﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TML.Files.Generic.Files;
using TML.Files.Generic.Utilities;
using TML.Files.Specific.Files;
using FileIO = System.IO.File;

namespace TML.Patcher.Backend.Packing
{
    public sealed class UnpackRequest
    {
        public DirectoryInfo ExtractDirectory { get; }

        public string FilePath { get; }

        public TModFile? File { get; private set; } = null;

        public double Threads { get; set; }

        public UnpackRequest(DirectoryInfo extractDirectory, string filePath, double threads)
        {
            ExtractDirectory = extractDirectory;
            FilePath = filePath;
            Threads = threads;
        }

        public void ExecuteRequest()
        {
            using (FileStream stream = FileIO.Open(FilePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                File = new TModFile(reader);
            }

            ExtractAllFiles(File.files, ExtractDirectory);
        }

        private void ExtractAllFiles(List<FileEntryData> files, FileSystemInfo extractDirectory)
        {
            List<List<FileEntryData>> chunks = new();

            if (Threads <= 0)
                Threads = 1D;

            double numThreads = Math.Min(files.Count, Threads); // Use either '4' threads, or the number of files, whatever is lower
            int chunkSize = (int)Math.Round(files.Count / numThreads, MidpointRounding.AwayFromZero);

            // Split the files into chunks
            for (int i = 0; i < files.Count; i += chunkSize)
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));


            // Run a task for each chunk
            // Wait for all tasks to finish
            Task.WaitAll(chunks.Select(chunk => Task.Run(() => ExtractChunkFiles(chunk, extractDirectory))).ToArray());
        }

        private static void ExtractChunkFiles(IEnumerable<FileEntryData> files, FileSystemInfo extractDirectory)
        {
            foreach (FileEntryData file in files)
            {
                byte[] data = file.fileData;

                if (file.fileLengthData.length != file.fileLengthData.lengthCompressed)
                    data = FileUtilities.DecompressFile(file.fileData, file.fileLengthData.length);

                string[] pathParts = file.fileName.Split(Path.DirectorySeparatorChar);
                string[] mendedPath = new string[pathParts.Length + 1];
                mendedPath[0] = extractDirectory.FullName;

                for (int i = 0; i < pathParts.Length; i++)
                    mendedPath[i + 1] = pathParts[i];

                string properPath = Path.Combine(mendedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(properPath) ?? string.Empty);

                if (Path.GetExtension(properPath) == ".rawimg")
                    FileConversion.ConvertRawToPng(data, properPath);
                else
                    FileIO.WriteAllBytes(properPath, data);
            }
        }
    }
}