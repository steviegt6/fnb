using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TML.Files;
using TML.Files.Utilities;

namespace TML.Patcher.Tasks
{
    public class UnpackTask : ProgressTask
    {
        public UnpackTask(DirectoryInfo extractDirectory, string path, double threads)
        {
            ExtractDirectory = extractDirectory;
            FilePath = path;
            Threads = threads;
        }

        public DirectoryInfo ExtractDirectory { get; }

        public string FilePath { get; }

        /// <summary>
        ///     <see cref="ModFile"/> instance of the file.
        /// </summary>
        public ModFile? ModFileInstance { get; private set; }

        /// <summary>
        ///     Amount of threads to use.
        /// </summary>
        public double Threads { get; set; }

        public override async Task ExecuteAsync()
        {
            await using (FileStream stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new(stream))
            {
                ModFileInstance = new ModFile(reader);
                
                ProgressReporter.Report("Populating ModFile.");
                
                ModFileInstance.PopulateFiles();
            }

            ExtractAllFiles(ModFileInstance.Files, ExtractDirectory);
        }

        /// <summary>
        ///     Extracts all <see cref="FileEntryData"/> instances.
        /// </summary>
        protected virtual void ExtractAllFiles(List<FileEntryData> files, FileSystemInfo extractDirectory)
        {
            List<List<FileEntryData>> chunks = new();

            if (Threads <= 0)
                Threads = 1D;

            // use either the amount of configured threads or the amount of files (whichever is lower)
            double numThreads = Math.Min(files.Count, Threads);
            int chunkSize = (int) Math.Round(files.Count / numThreads, MidpointRounding.AwayFromZero);

            // Split the files into chunks
            for (int i = 0; i < files.Count; i += chunkSize)
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));
            
            ProgressReporter.Report($"Processing {chunks.Count} chunk(s).");

            // Run a task for each chunk
            // Wait for all tasks to finish
            Task.WaitAll(chunks.Select(chunk => Task.Run(() => ExtractChunkFiles(chunk, extractDirectory))).ToArray());
        }

        /// <summary>
        ///     Extracts a chunk of given files. Used for multi-threading.
        /// </summary>
        protected virtual void ExtractChunkFiles(IEnumerable<FileEntryData> files, FileSystemInfo extractDirectory)
        {
            foreach (FileEntryData file in files)
            {
                byte[] data = file.FileData;

                if (file.FileLengthData.Length != file.FileLengthData.LengthCompressed)
                    data = FileUtilities.DecompressFile(file.FileData, file.FileLengthData.Length);

                string[] pathParts = file.FileName.Split(Path.DirectorySeparatorChar);
                string[] mendedPath = new string[pathParts.Length + 1];
                mendedPath[0] = extractDirectory.FullName;

                for (int i = 0; i < pathParts.Length; i++)
                    mendedPath[i + 1] = pathParts[i];

                string properPath = Path.Combine(mendedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(properPath) ?? string.Empty);

                if (Path.GetExtension(properPath) == ".rawimg")
                    FileConversion.ConvertRawToPng(data, properPath);
                else
                    File.WriteAllBytes(properPath, data);
            }
        }
    }
}