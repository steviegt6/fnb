using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TML.Files.Exceptions;
using TML.Files.Extensions;

namespace TML.Files.Extraction;

/// <summary>
///     Handles .tmod archive file extraction.
/// </summary>
public static class TModFileExtractor
{
    #region Extraction

    /// <summary>
    ///     Extracts the file entries within the given <paramref name="file"/> into a list of <see cref="TModFileData"/> records.
    /// </summary>
    /// <param name="file">The <see cref="TModFile"/> to extract.</param>
    /// <param name="threads">The amount of threads to use during extraction.</param>
    /// <param name="extractors">The <see cref="IFileExtractor"/>s to use for extraction.</param>
    /// <returns>A collection of extracted <see cref="TModFileData"/> records.</returns>
    public static List<TModFileData> Extract(TModFile file, int threads, params IFileExtractor[] extractors) {
        if (threads <= 0) threads = 1;

        List<List<TModFileEntry>> chunks = new();
        double numThreads = Math.Min(file.Entries.Count, threads);
        int chunkSize = (int) Math.Round(file.Entries.Count / numThreads, MidpointRounding.AwayFromZero);
        for (int i = 0; i < file.Entries.Count; i += chunkSize) chunks.Add(file.Entries.GetRange(i, Math.Min(chunkSize, file.Entries.Count - i)));

        ConcurrentBag<TModFileData> extractedFiles = new();
        Task.WaitAll(
            chunks.Select(chunk => Task.Run(() =>
                   {
                       IEnumerable<TModFileData> extracted = ExtractChunk(chunk, extractors);
                       foreach (TModFileData fileData in extracted) {
                            extractedFiles.Add(fileData);
                       }
                   }))
                  .ToArray()
        );

        return extractedFiles.ToList();
    }

    private static IEnumerable<TModFileData> ExtractChunk(List<TModFileEntry> entries, IFileExtractor[] extractors) {
        foreach (var entry in entries) {
            byte[] data = entry.Data ?? throw new TModFileInvalidFileEntryException("Attempted to serialize a TModFileEntry with no data: " + entry.Path);
            if (entry.IsCompressed()) data = Decompress(data);

            foreach (var extractor in extractors)
                if (extractor.ShouldExtract(entry)) {
                    yield return extractor.Extract(entry, data);
                    goto Continue;
                }

            throw new TModFileInvalidFileEntryException("No extractor found for file: " + entry.Path);

        Continue: ;
        }
    }

    private static byte[] Decompress(byte[] data) {
        using MemoryStream ms = new();
        using MemoryStream cs = new(data);
        using DeflateStream ds = new(cs, CompressionMode.Decompress);
        ds.CopyTo(ms);
        return ms.ToArray();
    }

    private static byte[] Compress(byte[] data) {
        using MemoryStream ms = new(data);
        using MemoryStream cs = new();
        using DeflateStream ds = new(cs, CompressionMode.Compress);
        ms.CopyTo(ds);
        return cs.ToArray();
    }

    #endregion

    #region Packing

    public static TModFile Pack(
        string directory,
        string modLoaderVersion,
        string modName,
        BuildProperties properties,
        uint minCompSize = TModFile.DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float minCompTradeoff = TModFile.DEFAULT_MINIMUM_COMPRESSION_TRADEOFF,
        params IFilePacker[] packers
    ) {
        directory = Path.GetFullPath(directory); // Ensure directory path is absolute since we shave it off to create a relative path later.

        if (!Directory.Exists(directory)) throw new TModFileDirectoryNotFoundException("Directory not found: " + directory);

        // Collect collection of paths (files) to pack.
        IEnumerable<string> resources = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Where(x => !properties.IsFileIgnored(x));

        // Read from absolute path but use relative path (shave off directory from absolute path) for the file data record, since it needs to be made relative to the .tmod archive.
        return Pack(
            resources.Select(x => new TModFileData(x.Substring(directory.Length + 1), File.ReadAllBytes(x))),
            modLoaderVersion,
            modName,
            properties,
            minCompSize,
            minCompTradeoff,
            packers
        );
    }

    public static TModFile Pack(
        IEnumerable<TModFileData> files,
        string modLoaderVersion,
        string modName,
        BuildProperties properties,
        uint minCompSize = TModFile.DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float minCompTradeoff = TModFile.DEFAULT_MINIMUM_COMPRESSION_TRADEOFF,
        params IFilePacker[] packers
    ) {
        var modFile = new TModFile
        {
            ModLoaderVersion = modLoaderVersion,
            Name = modName,
            Version = properties.Version.ToString()
        };

        foreach (var file in files) {
            var data = file;
            foreach (var packer in packers) {
                if (!packer.ShouldPack(data)) continue;
                data = packer.Pack(data);
                break;
            }

            modFile.AddFile(data, minCompSize, minCompTradeoff);
        }

        modFile.AddFile(new TModFileData("Info", properties.ToBytes(modLoaderVersion)), uint.MaxValue, 1f);
        return modFile;
    }

    private static void AddResource(TModFile file, string path) { }

    #endregion
}