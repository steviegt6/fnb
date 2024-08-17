using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;

using LibDeflate;

using Tomat.FNB.TMOD.Converters;

namespace Tomat.FNB.TMOD.Utilities;

internal static class TmodExtensions
{
    /// <summary>
    ///     Creates a read-only view into a <c>.tmod</c> file.
    /// </summary>
    /// <param name="tmod">The <c>.tmod</c> file to wrap.</param>
    /// <returns>A read-only view into the <c>.tmod</c> file.</returns>
    public static ReadOnlyTmodFile AsReadOnly(
        this IReadOnlyTmodFile tmod
    )
    {
        return new ReadOnlyTmodFile(tmod);
    }

    /// <summary>
    ///     Converts a <see cref="IReadOnlyTmodFile"/> to a
    ///     <see cref="ISerializableTmodFile"/>. Handles necessary file
    ///     conversion.
    /// </summary>
    /// <param name="tmod">The <c>.tmod</c> file.</param>
    /// <param name="converters">The file converters.</param>
    /// <param name="compress">Whether entries should be compressed.</param>
    /// <param name="minimumCompressionSize">
    ///     The minimum size of the file for it to be compressed.
    /// </param>
    /// <param name="minimumCompressionTradeoff">
    ///     The minimum tradeoff threshold for a file to be compressed.
    /// </param>
    /// <param name="maxDegreeOfParallelism">
    ///     The max degree of parallelism to use when converting files.
    /// </param>
    /// <returns>A serializable <c>.tmod</c> file ready to be written.</returns>
    /// <remarks>
    ///     <paramref name="compress"/> is overridden to <see langword="false"/>
    ///     if this <c>.tmod</c> file is written in a legacy format.
    /// </remarks>
    public static SerializableTmodFile ToSerializable(
        this IReadOnlyTmodFile tmod,
        IFileConverter[]       converters,
        bool                   compress                   = true,
        long                   minimumCompressionSize     = DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float                  minimumCompressionTradeoff = DEFAULT_MINIMUM_COMPRESSION_TRADEOFF,
        int                    maxDegreeOfParallelism     = -1
    )
    {
        if (maxDegreeOfParallelism == -1)
        {
            maxDegreeOfParallelism = Environment.ProcessorCount;
        }

        var isLegacy = Version.Parse(tmod.Version) < VERSION_0_11_0_0;
        if (isLegacy)
        {
            compress = false;
        }

        var entries = ConvertAndCompressEntries(
            tmod,
            converters,
            compress,
            minimumCompressionSize,
            minimumCompressionTradeoff,
            maxDegreeOfParallelism
        );

        var fileEntries = new Dictionary<string, ISerializableTmodFile.FileEntry>();
        {
            foreach (var (path, (originalBytes, compressedBytes)) in entries)
            {
                var entry = new ISerializableTmodFile.FileEntry
                {
                    Length           = originalBytes.Length,
                    CompressedLength = compressedBytes.Length,
                    Data             = compressedBytes,
                };
                fileEntries.Add(path, entry);
            }
        }

        return new SerializableTmodFile(
            tmod.ModLoaderVersion,
            tmod.Name,
            tmod.Version,
            fileEntries
        );
    }

    private static Dictionary<string, (byte[] originalBytes, byte[] compressedBytes)> ConvertAndCompressEntries(
        IReadOnlyTmodFile tmod,
        IFileConverter[]  converters,
        bool              compress,
        long              minimumCompressionSize,
        float             minimumCompressionTradeoff,
        int               maxDegreeOfParallelism
    )
    {
        var entries = new Dictionary<string, (byte[] originalBytes, byte[] compressedBytes)>();

        var transformBlock = new TransformBlock<
            (string path, byte[] data, IFileConverter[] converters, bool compress, long minimumCompressionSize, float minimumCompressionTradeoff),
            (string path, byte[] bytes)
        >(
            static obj => PackEntry(obj.path, obj.data, obj.converters, obj.compress, obj.minimumCompressionSize, obj.minimumCompressionTradeoff),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            }
        );
        {
            var action = new ActionBlock<(string path, byte[] bytes)>(
                obj =>
                {
                    entries.Add(obj.path, (tmod.Entries[obj.path], obj.bytes));
                }
            );
            transformBlock.LinkTo(
                action,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true,
                }
            );

            foreach (var (path, entry) in tmod.Entries)
            {
                transformBlock.Post((path, entry, converters, compress, minimumCompressionSize, minimumCompressionTradeoff));
            }

            transformBlock.Complete();
            action.Completion.Wait();
        }

        return entries;
    }

    private static (string path, byte[] bytes) ExtractEntry(
        string                          path,
        ISerializableTmodFile.FileEntry entry,
        IFileConverter[]                converters
    )
    {
        Debug.Assert(entry.Data is not null);

        entry = entry with { Data = Decompress(entry.Data, entry.Length) };
        return ProcessEntry(path, entry.Data, converters);
    }

    private static (string path, byte[] bytes) PackEntry(
        string           path,
        byte[]           data,
        IFileConverter[] converters,
        bool             compress                   = true,
        long             minimumCompressionSize     = DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float            minimumCompressionTradeoff = DEFAULT_MINIMUM_COMPRESSION_TRADEOFF
    )
    {
        (path, data) = ProcessEntry(path, data, converters);

        if (!compress || data.Length < minimumCompressionSize)
        {
            return (path, data);
        }

        var compressedBytes = Compress(data);
        var meetsTradeoff   = compressedBytes.Length < data.Length * minimumCompressionTradeoff;
        return (path, meetsTradeoff ? compressedBytes : data);
    }

    private static (string path, byte[] bytes) ProcessEntry(
        string           path,
        byte[]           data,
        IFileConverter[] converters
    )
    {
        foreach (var converter in converters)
        {
            if (!converter.ShouldConvert(path, data))
            {
                continue;
            }

            (path, var bytes) = converter.Convert(path, data);
            return (path, bytes);
        }

        return (path, data);
    }

    private static byte[] Decompress(byte[] data, int uncompressedLength)
    {
        // In cases where the file isn't actually compressed.  This is possible
        // with custom fnb settings or if a tModLoader `.tmod` contains files
        // small enough to not be compressed.
        if (data.Length == uncompressedLength)
        {
            return data;
        }

        // TODO(perf): Benchmark again to confirm this is worthwhile.
        //             The garbage collector already tries to zero-initialize
        //             ahead of time but we can end up allocating rather large
        //             arrays...
        // We can skip zero-initialization because we overwrite all the data in
        // the allocated array regardless.
        var array = GC.AllocateUninitializedArray<byte>(uncompressedLength);

        using var ds = new DeflateDecompressor();
        {
            ds.Decompress(data, new Span<byte>(array), out var written);
            {
                Debug.Assert(written == uncompressedLength && array.Length == uncompressedLength);
            }
        }

        return data;
    }

    private static byte[] Compress(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using (var ds = new DeflateStream(ms, CompressionMode.Compress))
        {
            ds.Write(data, 0, data.Length);
        }

        // TODO(perf): Prefer GetBuffer?
        var compressedData = ms.ToArray();
        return compressedData;
    }
}