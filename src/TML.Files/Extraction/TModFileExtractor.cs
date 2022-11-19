using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TML.Files.Exceptions;
using TML.Files.Extensions;

namespace TML.Files.Extraction;

public static class TModFileExtractor
{
    #region Extraction

    public static List<TModFileData> Extract(TModFile file, int threads, params IFileExtractor[] extractors) {
        if (threads <= 0) threads = 1;

        List<List<TModFileEntry>> chunks = new();
        double numThreads = Math.Min(file.Entries.Count, threads);
        int chunkSize = (int) Math.Round(file.Entries.Count / numThreads, MidpointRounding.AwayFromZero);
        for (int i = 0; i < file.Entries.Count; i += chunkSize) chunks.Add(file.Entries.GetRange(i, Math.Min(chunkSize, file.Entries.Count - i)));

        List<TModFileData> extractedFiles = new();
        Task.WaitAll(chunks.Select(chunk => Task.Run(() =>
        {
            IEnumerable<TModFileData> extracted = ExtractChunk(chunk, extractors);
            lock (extractedFiles) extractedFiles.AddRange(extracted);
        })).ToArray());

        return extractedFiles;
    }

    private static IEnumerable<TModFileData> ExtractChunk(List<TModFileEntry> entries, IFileExtractor[] extractors) {
        foreach (var entry in entries) {
            byte[] data = entry.Data ?? throw new TModFileInvalidFileEntryException("Attempted to serialize a TModFileEntry with no data: " + entry.Path);
            if (entry.IsCompressed()) data = Deflate(data, CompressionMode.Decompress);
            
            foreach (var extractor in extractors) {
                if (extractor.ShouldExtract(entry)) yield return extractor.Extract(entry, data);
                throw new TModFileInvalidFileEntryException("No extractor found for file: " + entry.Path);
            }
        }
    }

    private static byte[] Deflate(byte[] data, CompressionMode mode) {
        using MemoryStream ms = new(data);
        using MemoryStream cs = new();
        using DeflateStream ds = new(cs, mode);
        ms.CopyTo(ds);
        return cs.ToArray();
    }

    #endregion

    #region Packing

    #endregion
}