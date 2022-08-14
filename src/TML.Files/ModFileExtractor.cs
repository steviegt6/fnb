using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TML.Files.Abstractions;
using TML.Files.Abstractions.Extensions;

namespace TML.Files
{
    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IModFileExtractor"/> implementation.
    /// </summary>
    public class ModFileExtractor : IModFileExtractor
    {
        public double Threads { get; set; } = 8.0;

        public IEnumerable<IExtractedModFile> Extract(IModFile file, params IFileExtractor[] fileExtractors) {
            if (Threads <= 0) Threads = 1.0;
            
            List<IModFileEntry> files = file.Files.ToList();
            List<List<IModFileEntry>> chunks = new();
            double numThreads = Math.Min(files.Count, Threads); // Use either configured thread count or file count, whichever is lower.
            int chunkSize = (int) Math.Round(files.Count / numThreads, MidpointRounding.AwayFromZero);
            
            // Split files into chunks.
            for (int i = 0; i < files.Count; i += chunkSize) {
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));
            }
            
            List<IExtractedModFile> extractedFiles = new();
            Task.WaitAll(chunks.Select(chunk => Task.Run(() =>
            {
                IEnumerable<IExtractedModFile> extracted = ExtractChunk(chunk, fileExtractors);
                
                // TODO: Is this lock necessary?
                lock (extractedFiles) {
                    extractedFiles.AddRange(extracted);
                }
            })).ToArray());

            return extractedFiles;
        }

        protected static IEnumerable<IExtractedModFile> ExtractChunk(IEnumerable<IModFileEntry> files, IFileExtractor[] fileExtractors) {
            foreach (IModFileEntry entry in files) {
                byte[] data = entry.CachedBytes ?? Array.Empty<byte>();
                if (entry.Compressed()) data = Decompress(data);

                foreach (IFileExtractor extractor in fileExtractors) {
                    if (extractor.ShouldExtract(entry)) {
                        yield return extractor.Extract(entry, data);
                    }
                    
                    // TODO: Throw if no valid extractors present?
                }
            }
        }
        
        protected static byte[] Decompress(byte[] data) {
            using MemoryStream decompressedStream = new();
            using MemoryStream compressStream = new(data);
            using DeflateStream deflateStream = new(compressStream, CompressionMode.Decompress);
            deflateStream.CopyTo(decompressedStream);
            return decompressedStream.ToArray();
        }

        protected static byte[] Compress(byte[] data) {
            using MemoryStream dataStream = new(data);
            using MemoryStream compressStream = new();
            using DeflateStream deflateStream = new(compressStream, CompressionMode.Compress);
            dataStream.CopyTo(deflateStream);
            return compressStream.ToArray();
        }
    }
}