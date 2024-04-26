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
using Tomat.FNB.Common.Utilities;
using Tomat.FNB.TMOD.Extractors;

namespace Tomat.FNB.TMOD;

/// <summary>
///     Minimal information pertaining to file data within a .tmod archive.
/// </summary>
/// <param name="Path">The file path within the archive.</param>
/// <param name="Data">The file data.</param>
public readonly record struct TmodFileData(string Path, AmbiguousData<byte> Data);

/// <summary>
///     An actual file entry within a .tmod archive.
/// </summary>
/// <param name="Path">The file path within the archive.</param>
/// <param name="Offset">The offset of the data within the archive.</param>
/// <param name="Length">The length of the data within the archive.</param>
/// <param name="CompressedLength">
///     The compressed length of the data within the archive.
/// </param>
/// <param name="Data">The data of the entry within the archive.</param>
public readonly record struct TmodFileEntry(string Path, int Offset, int Length, int CompressedLength, AmbiguousData<byte>? Data);

/// <summary>
///     Represents a .tmod archive.
/// </summary>
/// <param name="modLoaderVersion">
///     The tModLoader version this file was created by.
/// </param>
/// <param name="name">
///     The name of the mod this archive contains.
/// </param>
/// <param name="version">
///     The version of the mod this archive contains.
/// </param>
/// <param name="entries">
///     The files in the archive.
/// </param>
public sealed class TmodFile(string modLoaderVersion, string name, string version, List<TmodFileEntry> entries) {
    /// <summary>
    ///     The default minimum size for a file to be compressed.
    /// </summary>
    public const uint DEFAULT_MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1 KiB

    /// <summary>
    ///     The default minimum tradeoff for a file to be compressed.
    /// </summary>
    public const float DEFAULT_MINIMUM_COMPRESSION_TRADEOFF = 0.9f;

    /// <summary>
    ///     The header of a .tmod file.
    /// </summary>
    public const uint TMOD_HEADER = 0x444F4D54; // 0x544D4F44; // "TMOD"

    /// <summary>
    ///     The length of the hash in bytes.
    /// </summary>
    private const int hash_length = 20;

    /// <summary>
    ///     The length of the signature in bytes.
    /// </summary>
    private const int signature_length = 256;

    /// <summary>
    ///     A collection of extensions that should not be compressed.
    /// </summary>
    private static readonly string[] extensions_to_not_compress = { ".png", ".mp3", ".ogg" };

    /// <summary>
    ///     Version of an older format to upgrade from.
    /// </summary>
    private static readonly Version upgrade_version = new(0, 11, 0, 0);

    /// <summary>
    ///     The file extractors to use.
    /// </summary>
    private static readonly FileExtractor[] extractors;

    // ReSharper disable ConvertToConstant.Local - avoid allocations.
    private static readonly char dirty_separator = '\\';
    private static readonly char clean_separator = '/';
    // ReSharper restore ConvertToConstant.Local

    static TmodFile() {
        FileExtractor rawimgExtractor;
        if (OperatingSystem.IsWindows() && Environment.Is64BitProcess && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            rawimgExtractor = new FpngExtractor();
        else
            rawimgExtractor = new RawImgFileExtractor();

        extractors = new[] { rawimgExtractor, new InfoFileExtractor() };
    }

    /// <summary>
    ///     The version of tModLoader this file was created by.
    /// </summary>
    public string ModLoaderVersion { get; } = modLoaderVersion;

    /// <summary>
    ///     The name of the mod this archive contains.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     The version of the mod this archive contains.
    /// </summary>
    public string Version { get; } = version;

    /// <summary>
    ///     The files in the archive.
    /// </summary>
    public List<TmodFileEntry> Entries { get; } = entries;

    /// <summary>
    ///     Adds a file to the archive.
    /// </summary>
    /// <param name="fileData">The file data to add.</param>
    /// <param name="minCompSize">
    ///     The minimum size for a file to be compressed.
    /// </param>
    /// <param name="minCompTradeoff">
    ///     The minimum tradeoff for a file to be compressed.
    /// </param>
    public void AddFile(TmodFileData fileData, uint minCompSize = DEFAULT_MINIMUM_COMPRESSION_SIZE, float minCompTradeoff = DEFAULT_MINIMUM_COMPRESSION_TRADEOFF) {
        // Sanitize paths.
        fileData = fileData with {
            Path = fileData.Path.Trim().Replace(dirty_separator, clean_separator),
        };

        // Handle compression if it's allowed.
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

    /// <summary>
    ///     Writes the .tmod file to a stream.
    /// </summary>
    /// <param name="stream">
    ///     The stream to write to.
    /// </param>
    /// <returns>
    ///     Whether the write was successful.
    /// </returns>
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
                    writer.Write(entry.Data!.Span);
                }
            }
            else {
                foreach (var entry in Entries) {
                    writer.Write(entry.Path);
                    writer.Write(entry.CompressedLength);
                    writer.Write(entry.Length);
                }

                foreach (var entry in Entries)
                    writer.Write(entry.Data!.Span);
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
        var data = fileData.Data;
        using var ms = new MemoryStream(data.Array);
        using (var ds = new DeflateStream(ms, CompressionMode.Compress))
            ds.Write(data.Array, 0, fileData.Data.Length);

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

    public void Extract(ActionBlock<TmodFileData> finalBlock, int maxDegreeOfParallelism) {
        var transformBlock = new TransformBlock<TmodFileEntry, TmodFileData>(
            ProcessModEntry,
            new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = maxDegreeOfParallelism == -1 ? Environment.ProcessorCount : maxDegreeOfParallelism,
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
        var array = GC.AllocateUninitializedArray<byte>(uncompressedLength);
        using DeflateDecompressor ds = new();
        ds.Decompress(data.Array, new Span<byte>(array), out _);
        return new AmbiguousData<byte>(array);
    }
}
