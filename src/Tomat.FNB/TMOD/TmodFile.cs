using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using LibDeflate;
using Tomat.FNB.TMOD.Extractors;
using Tomat.FNB.Util;

namespace Tomat.FNB.TMOD;

public sealed class TmodFile (string modLoaderVersion, string name, string version, List<TmodFileEntry> entries) {
    public const uint DEFAULT_MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1 KiB
    public const float DEFAULT_MINIMUM_COMPRESSION_TRADEOFF = 0.9f;
    public const uint TMOD_HEADER = 0x444F4D54; // 0x544D4F44; // "TMOD"

    private const int hash_length = 20;
    private const int signature_length = 256;
    private static readonly string[] extensions_to_not_compress = { ".png", ".mp3", ".ogg" };
    private static readonly Version upgrade_version = new(0, 11, 0, 0);
    private static readonly FileExtractor[] extractors;

    // ReSharper disable once ConvertToConstant.Local - avoid allocations.
    private static readonly char dirty_separator = '\\';

    // ReSharper disable once ConvertToConstant.Local - avoid allocations.
    private static readonly char clean_separator = '/';

    static TmodFile() {
        FileExtractor rawimgExtractor;
        if (OperatingSystem.IsWindows() && Environment.Is64BitProcess && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            rawimgExtractor = new FpngExtractor();
        else
            rawimgExtractor = new RawImgFileExtractor();

        extractors = new[] { rawimgExtractor, new InfoFileExtractor() };
    }

    public string ModLoaderVersion { get; } = modLoaderVersion;

    public string Name { get; } = name;

    public string Version { get; } = version;

    public List<TmodFileEntry> Entries { get; } = entries;

    public void AddFile(TmodFileData fileData, uint minCompSize = DEFAULT_MINIMUM_COMPRESSION_SIZE, float minCompTradeoff = DEFAULT_MINIMUM_COMPRESSION_TRADEOFF) {
        fileData = fileData with {
            Path = fileData.Path.Trim().Replace(dirty_separator, clean_separator),
        };

        var size = fileData.Data.Length;
        if (size > minCompSize && ShouldCompress(fileData))
            Compress(ref fileData, size, minCompTradeoff);

        Entries.Add(
            new TmodFileEntry(
                fileData.Path,
                -1,
                size,
                fileData.Data.Length,
                fileData.Data
            )
        );
    }

    public bool TryWrite(Stream stream) {
        var writer = new BinaryWriter(stream);

        try {
            writer.Write(TMOD_HEADER);
            writer.Write(ModLoaderVersion);

            var hashStartPos = stream.Position;
            writer.Write(new byte[hash_length]);
            writer.Write(new byte[signature_length]);
            writer.Write(0);

            var hashEndPos = stream.Position;

            var legacy = new Version(ModLoaderVersion) < upgrade_version;

            if (legacy) {
                var ms = new MemoryStream();
                var ds = new DeflateStream(ms, CompressionMode.Compress, true);
                writer = new BinaryWriter(ds);
            }

            writer.Write(Name);
            writer.Write(Version);
            writer.Write(Entries.Count);

            if (legacy) {
                foreach (var entry in Entries) {
                    writer.Write(entry.Path);
                    writer.Write(entry.Length);
                    writer.WriteAmbiguousData(entry.Data!);
                }
            }
            else {
                foreach (var entry in Entries) {
                    writer.Write(entry.Path);
                    writer.Write(entry.CompressedLength);
                    writer.Write(entry.Length);
                }

                foreach (var entry in Entries)
                    writer.WriteAmbiguousData(entry.Data!);
            }

            if (legacy) {
                var compressed = ((MemoryStream) writer.BaseStream).GetBuffer();
                writer.Dispose();
                writer = new BinaryWriter(stream);
                writer.Write(compressed);
            }

            stream.Position = hashEndPos;
            var hash = SHA1.Create().ComputeHash(stream);

            stream.Position = hashStartPos;
            writer.Write(hash);
            writer.Write(new byte[signature_length]);
            writer.Write((int) (stream.Length - hashEndPos));

            return true;
        }
        finally {
            writer.Dispose();
        }
    }

    private static bool ShouldCompress(TmodFileData fileData) {
        return !extensions_to_not_compress.Contains(Path.GetExtension(fileData.Path));
    }

    private static void Compress(ref TmodFileData fileData, int realSize, float tradeoff) {
        var data = fileData.Data.ToArray();
        using var ms = new MemoryStream(data);
        using (var ds = new DeflateStream(ms, CompressionMode.Compress))
            ds.Write(data, 0, fileData.Data.Length);

        var compressed = ms.GetBuffer();

        if (compressed.Length < realSize * tradeoff) {
            fileData = fileData with {
                Data = new AmbiguousData<byte>(compressed),
            };
        }
    }

    public static bool TryReadFromPath(string path, [NotNullWhen(returnValue: true)] out TmodFile? tmodFile) {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return TryReadFromStream(fs, out tmodFile);
    }

    public static bool TryReadFromArray(byte[] b, [NotNullWhen(returnValue: true)] out TmodFile? tmodFile) {
        using var ms = new MemoryStream(b);
        return TryReadFromStream(ms, out tmodFile);
    }

    public static bool TryReadFromStream(Stream stream, [NotNullWhen(returnValue: true)] out TmodFile? tmodFile) {
        var reader = new BinaryReader(stream);
        tmodFile = null;

        try {
            if (reader.ReadUInt32() != TMOD_HEADER)
                return false;

            var modLoaderVersion = reader.ReadString();

            /*
            _ = reader.ReadBytes(hash_length);
            _ = reader.ReadBytes(signature_length);
            _ = reader.ReadUInt32();
            */
            stream.Position += hash_length;
            stream.Position += signature_length;
            stream.Position += sizeof(uint);

            var legacy = new Version(modLoaderVersion) < upgrade_version;

            if (legacy) {
                var ds = new DeflateStream(stream, CompressionMode.Decompress, true);
                reader = new BinaryReader(ds);
            }

            var name = reader.ReadString();
            var version = reader.ReadString();

            var offset = 0;
            var entries = new TmodFileEntry[reader.ReadInt32()];

            if (legacy) {
                for (var i = 0; i < entries.Length; i++) {
                    var entryName = reader.ReadString();
                    var entrySize = reader.ReadInt32();
                    var entryData = reader.ReadBytes(entrySize);

                    entries[i] = new TmodFileEntry(entryName, offset, entrySize, entrySize, new AmbiguousData<byte>(entryData));
                }
            }
            else {
                for (var i = 0; i < entries.Length; i++) {
                    entries[i] = new TmodFileEntry(reader.ReadString(), offset, reader.ReadInt32(), reader.ReadInt32(), null);
                    offset += entries[i].CompressedLength;
                }

                if (stream.Position >= int.MaxValue)
                    return false;

                var fileStartPos = (int) stream.Position;

                for (var i = 0; i < entries.Length; i++) {
                    var entry = entries[i];
                    entries[i] = entries[i] with {
                        Offset = entry.Offset + fileStartPos,
                        Data = new AmbiguousData<byte>(reader.ReadBytes(entry.CompressedLength)),
                    };
                }
            }

            tmodFile = new TmodFile(modLoaderVersion, name, version, entries.ToList());
            return true;
        }
        finally {
            reader.Dispose();
        }
    }

    public List<TmodFileData> Extract() {
        var files = new List<TmodFileData>();
        Extract(new ActionBlock<TmodFileData>(files.Add));
        return files;
    }

    public void Extract(ActionBlock<TmodFileData> finalBlock) {
        var transformBlock = new TransformBlock<TmodFileEntry, TmodFileData>(
            ProcessModEntry,
            new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount * 5 / 8),
            }
        );

        var linkOptions = new DataflowLinkOptions {
            PropagateCompletion = true,
        };

        transformBlock.LinkTo(finalBlock, linkOptions);

        foreach (var entry in Entries)
            transformBlock.Post(entry);

        transformBlock.Complete();
        finalBlock.Completion.Wait();
    }

    private static TmodFileData ProcessModEntry(TmodFileEntry entry) {
        var data = entry.Data!;
        if (data.Length != entry.Length)
            data = Decompress(data, entry.Length);

        foreach (var extractor in extractors) {
            if (extractor.ShouldExtract(entry))
                return extractor.Extract(entry, data);
        }

        return new TmodFileData(entry.Path, data);
    }

    private static AmbiguousData<byte> Decompress(AmbiguousData<byte> data, int uncompressedLength) {
        using MemoryStream ms = new();
        using DeflateDecompressor ds = new();
        ds.Decompress(data.ToArray(), uncompressedLength, out var ownedMemory);
        return new AmbiguousData<byte>(ownedMemory!.Memory);
    }

    private static byte[] Compress(byte[] data) {
        using MemoryStream ms = new(data);
        using MemoryStream cs = new();
        using DeflateStream ds = new(cs, CompressionMode.Compress);
        ms.CopyTo(ds);
        return cs.GetBuffer();
    }
}
