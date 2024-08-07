using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using LibDeflate;

using Tomat.FNB.TMOD.Converters;

namespace Tomat.FNB.TMOD;

public static class TmodFileSerializer
{
    public readonly record struct WriteOptions(
        IFileConverter[] Converters,
        bool             Compress                   = true,
        long             MinimumCompressionSize     = DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float            MinimumCompressionTradeoff = DEFAULT_MINIMUM_COMPRESSION_TRADEOFF
    );

    public readonly record struct ReadOptions(
        IFileConverter[]           Converters,
        Func<string, byte[], Task> Processor,
        int                        InputDegree,
        int                        OutputDegree
    );

    private record struct TmodFileEntry(
        string  Path,
        int     Length,
        int     CompressedLength,
        byte[]? Data
    );

#region Read
    public static ITmodFile Read(string path, ReadOptions opts)
    {
        using var fs = File.OpenRead(path);
        return Read(fs, opts);
    }

    public static ITmodFile Read(byte[] bytes, ReadOptions opts)
    {
        using var ms = new MemoryStream(bytes);
        return Read(ms, opts);
    }

    public static ITmodFile Read(Stream stream, ReadOptions opts)
    {
        var reader = new BinaryReader(stream);

        try
        {
            if (reader.ReadUInt32() != TMOD_HEADER)
            {
                throw new InvalidDataException("Invalid TMOD header!");
            }

            var modLoaderVersion = reader.ReadString();

            stream.Position += HASH_LENGTH
                             + SIGNATURE_LENGTH
                             + sizeof(uint);

            var isLegacy = Version.Parse(modLoaderVersion) < VERSION_0_11_0_0;
            if (isLegacy)
            {
                var ds = new DeflateStream(stream, CompressionMode.Decompress, true);
                reader = new BinaryReader(ds);
            }

            var name    = reader.ReadString();
            var version = reader.ReadString();

            var entries = new TmodFileEntry[reader.ReadInt32()];

            if (isLegacy)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    var path   = reader.ReadString();
                    var length = reader.ReadInt32();
                    var data   = reader.ReadBytes(length);

                    entries[i] = new TmodFileEntry(path, length, length, data);
                }
            }
            else
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    entries[i] = new TmodFileEntry(
                        reader.ReadString(),
                        reader.ReadInt32(),
                        reader.ReadInt32(),
                        null
                    );
                }

                if (stream.Position >= int.MaxValue)
                {
                    throw new InvalidDataException("TMOD file is too large!");
                }

                for (var i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];

                    entries[i] = entries[i] with
                    {
                        Data = reader.ReadBytes(entry.CompressedLength),
                    };
                }
            }

            Debug.Assert(!entries.Any(x => x.Data is null));

            Dictionary<string, byte[]> realEntries = [];
            {
                var transformBlock = new TransformBlock<(TmodFileEntry, IFileConverter[]), (string, byte[])>(
                    ProcessEntry,
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = opts.InputDegree,
                    }
                );

                var linkOptions = new DataflowLinkOptions
                {
                    PropagateCompletion = true,
                };

                var block = new ActionBlock<(string path, byte[] data)>(
                    async obj =>
                    {
                        await opts.Processor(obj.path, obj.data);
                        realEntries[obj.path] = obj.data;
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = opts.OutputDegree,
                    }
                );
                transformBlock.LinkTo(block, linkOptions);

                foreach (var entry in entries)
                {
                    transformBlock.Post((entry, opts.Converters));
                }

                transformBlock.Complete();
                block.Completion.Wait();
            }

            return new TmodFile(modLoaderVersion, name, version, realEntries);
        }
        finally
        {
            reader.Dispose();
        }
    }
#endregion

#region Write
    public static void Write(ITmodFile tmodFile, string path, WriteOptions opts)
    {
        using var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
        Write(tmodFile, fs, opts);
    }

    public static void Write(ITmodFile tmodFile, Stream stream, WriteOptions opts)
    {
        Dictionary<string, byte[]> entries = [];
        foreach (var (path, data) in tmodFile.Entries)
        {
            var (realPath, realData) = Convert(path, data, opts.Converters);
            entries[realPath]        = realData;
        }

        var writer = new BinaryWriter(stream);

        try
        {
            writer.Write(TMOD_HEADER);
            writer.Write(tmodFile.ModLoaderVersion);

            var hashStartPos = stream.Position;
            {
                // writer.Write(new byte[HASH_LENGTH]);
                // writer.Write(new byte[SIGNATURE_LENGTH]);
                // writer.Write(0);

                writer.Write(new byte[HASH_LENGTH + SIGNATURE_LENGTH + sizeof(uint)]);
            }
            var hashEndPos = stream.Position;

            var isLegacy = Version.Parse(tmodFile.ModLoaderVersion) < VERSION_0_11_0_0;
            if (isLegacy)
            {
                var ms = new MemoryStream();
                var ds = new DeflateStream(ms, CompressionMode.Compress, true);
                writer = new BinaryWriter(ds);
            }

            writer.Write(tmodFile.Name);
            writer.Write(tmodFile.Version);
            writer.Write(tmodFile.Entries.Count);

            if (isLegacy)
            {
                foreach (var (path, data) in entries)
                {
                    writer.Write(path);
                    writer.Write(data.Length);
                    writer.Write(data);
                }
            }
            else
            {
                var compressedData = new byte[][entries.Count];

                var i = 0;
                foreach (var (path, data) in entries)
                {
                    compressedData[i] = opts.Compress ? Compress(data, opts) : data;

                    writer.Write(path);
                    writer.Write(data.Length);
                    writer.Write(compressedData[i].Length);

                    i++;
                }

                for (i = 0; i < compressedData.Length; i++)
                {
                    writer.Write(compressedData[i]);
                }
            }

            if (isLegacy)
            {
                Debug.Assert(writer.BaseStream is MemoryStream);

                // TODO: Can we replace ToArray with GetBuffer?
                var compressed = (writer.BaseStream as MemoryStream)!.ToArray();
                writer.Dispose();
                writer = new BinaryWriter(stream);
                writer.Write(compressed);
            }

            stream.Position = hashEndPos;
            {
                var hash = SHA1.Create().ComputeHash(stream);

                stream.Position = hashStartPos;
                {
                    writer.Write(hash);
                    writer.Write(new byte[SIGNATURE_LENGTH]);
                    writer.Write((int)(stream.Length - hashEndPos));
                }
            }
        }
        finally
        {
            writer.Dispose();
        }
    }
#endregion

    private static (string path, byte[] data) Convert(string path, byte[] data, IFileConverter[] converters)
    {
        foreach (var converter in converters)
        {
            if (converter.ShouldConvert(path, data))
            {
                return converter.Convert(path, data);
            }
        }

        return (path, data);
    }

    private static (string, byte[]) ProcessEntry((TmodFileEntry, IFileConverter[]) obj)
    {
        var (entry, converters) = obj;
        Debug.Assert(entry.Data is not null);

        var data = Decompress(entry.Data, entry.Data.Length);
        return Convert(entry.Path, data, converters);
    }

    private static byte[] Decompress(byte[] data, int uncompressedLength)
    {
        // In cases where the file isn't actually compressed.  This is possible
        // for smaller files.
        if (data.Length == uncompressedLength)
        {
            return data;
        }

        var array = GC.AllocateUninitializedArray<byte>(uncompressedLength);

        using var ds = new DeflateDecompressor();
        ds.Decompress(data, new Span<byte>(array), out var written);
        {
            Debug.Assert(written == uncompressedLength && array.Length == uncompressedLength);
        }

        return data;
    }

    private static byte[] Compress(byte[] data, WriteOptions opts)
    {
        if (data.Length < opts.MinimumCompressionSize)
        {
            return data;
        }

        using var ms = new MemoryStream(data);
        using (var ds = new DeflateStream(ms, CompressionMode.Compress))
        {
            ds.Write(data, 0, data.Length);
        }

        // TODO: Can we replace ToArray with GetBuffer?
        var compressed = ms.ToArray();
        return compressed.Length < data.Length * opts.MinimumCompressionTradeoff ? compressed : data;
    }
}