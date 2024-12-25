using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Dataflow;

using Tomat.FNB.TMOD.Converters;

namespace Tomat.FNB.TMOD.Utilities;

public static partial class TmodExtensions
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
        if (maxDegreeOfParallelism < 0)
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

    /// <summary>
    ///     Uses the given <paramref name="converters"/> to convert the
    ///     <paramref name="tmod"/> file['s entries].
    /// </summary>
    /// <param name="tmod">The <c>.tmod</c> file.</param>
    /// <param name="converters">The file converters.</param>
    /// <param name="action">An additional action that may be performed.</param>
    /// <param name="transformParallelism">
    ///     The max degree of parallelism to use when converting files.
    /// </param>
    /// <param name="actionParallelism">
    ///     The max degree of parallelism to use when performing actions.
    /// </param>
    /// <returns>
    ///     A new <c>.tmod</c> file with the converted entries.
    /// </returns>
    public static TmodFile Convert(
        this ISerializableTmodFile                      tmod,
        IFileConverter[]                                converters,
        Action<Action<string, byte[]>, string, byte[]>? action               = null,
        int                                             transformParallelism = -1,
        int                                             actionParallelism    = -1
    )
    {
        const int numerator   = 6;
        const int denominator = 8;

        if (transformParallelism < 0 && actionParallelism < 0)
        {
            transformParallelism = Math.Max(1, Environment.ProcessorCount * numerator                 / denominator);
            actionParallelism    = Math.Max(1, Environment.ProcessorCount * (denominator - numerator) / denominator);
        }
        else if (transformParallelism < 0)
        {
            if (actionParallelism >= Environment.ProcessorCount)
            {
                actionParallelism = Math.Max(1, Environment.ProcessorCount - 1);
            }

            transformParallelism = Environment.ProcessorCount - actionParallelism;
        }
        else if (actionParallelism < 0)
        {
            if (transformParallelism >= Environment.ProcessorCount)
            {
                transformParallelism = Math.Max(1, Environment.ProcessorCount - 1);
            }

            actionParallelism = Environment.ProcessorCount - transformParallelism;
        }

        var entries = ConvertAndDecompressEntries(
            tmod,
            converters,
            action,
            transformParallelism,
            actionParallelism
        );

        return new TmodFile(
            tmod.ModLoaderVersion,
            tmod.Name,
            tmod.Version,
            entries
        );
    }

    private static Dictionary<string, byte[]> ConvertAndDecompressEntries(
        ISerializableTmodFile                           tmod,
        IFileConverter[]                                converters,
        Action<Action<string, byte[]>, string, byte[]>? action,
        int                                             transformParallelism,
        int                                             actionParallelism
    )
    {
        var entries = new Dictionary<string, byte[]>();

        var transformBlock = new TransformBlock<
            (string path, ISerializableTmodFile.FileEntry entry, IFileConverter[] converters),
            (string path, byte[] bytes)>(
            static obj => ExtractEntry(obj.path, obj.entry, obj.converters),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = transformParallelism,
            }
        );
        {
            var concurrentEntries = new ConcurrentDictionary<string, byte[]>();

            var addFile = new Action<string, byte[]>((path, bytes) => concurrentEntries.AddOrUpdate(path, bytes, (_, _) => bytes));
            action ??= (act, path, bytes) =>
            {
                act(path, bytes);
            };

            var actionBlock = new ActionBlock<(string path, byte[] bytes)>(
                obj =>
                {
                    action(addFile, obj.path, obj.bytes);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = actionParallelism,
                }
            );
            transformBlock.LinkTo(
                actionBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true,
                }
            );

            foreach (var (path, entry) in tmod.Entries)
            {
                transformBlock.Post((path, entry, converters));
            }

            transformBlock.Complete();
            actionBlock.Completion.Wait();

            foreach (var (path, bytes) in concurrentEntries)
            {
                entries.Add(path, bytes);
            }
        }

        return entries;

        static (string path, byte[] bytes) ExtractEntry(
            string                          path,
            ISerializableTmodFile.FileEntry entry,
            IFileConverter[]                converters
        )
        {
            Debug.Assert(entry.Data is not null);

            var decompressed = Decompress(entry.Data, entry.Length);
            return ProcessEntry(path, decompressed, converters);
        }
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

        static (string path, byte[] bytes) PackEntry(
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

    private static unsafe byte[] Decompress(byte[] data, int uncompressedLength)
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

        // using var ds = new DeflateDecompressor();
        // {
        //     ds.Decompress(data, new Span<byte>(array), out var written);
        //     {
        //         Debug.Assert(written == uncompressedLength && array.Length == uncompressedLength);
        //     }
        // }

        fixed (byte* inData = data)
        {
            fixed (byte* outData = array)
            {
                var written = decompress_deflate(inData, data.Length, outData, uncompressedLength);
                {
                    Debug.Assert(written == uncompressedLength);
                }
            }
        }

        return array;
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
    
    [LibraryImport("libfnb", EntryPoint = "decompress_deflate")]
    private static unsafe partial nint decompress_deflate(byte* in_data, nint in_length, byte* out_data, nint out_length);
}