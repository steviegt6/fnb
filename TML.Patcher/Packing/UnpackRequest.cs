using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TML.Files.Generic.Files;
using TML.Files.Generic.Utilities;
using TML.Files.ModLoader.Files;
using FileIO = System.IO.File;

namespace TML.Patcher.Packing
{
    /// <summary>
    ///     .tmod file unpacking request.
    /// </summary>
    public class UnpackRequest
    {
        /// <summary>
        ///     Constructs a new <see cref="UnpackRequest"/> instance.
        /// </summary>
        public UnpackRequest(DirectoryInfo extractDirectory, string path, double threads,
            IProgress<int> progressReporter)
        {
            ExtractDirectory = extractDirectory;
            FilePath = path;
            Threads = threads;
            ProgressReporter = progressReporter;
        }

        /// <summary>
        ///     The extraction directory.
        /// </summary>
        public DirectoryInfo ExtractDirectory { get; }

        /// <summary>
        ///     Path of the file to unpack.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     <see cref="ModFile"/> instance of the file.
        /// </summary>
        public ModFile? File { get; private set; }

        /// <summary>
        ///     Amount of threads to use.
        /// </summary>
        public double Threads { get; set; }

        /// <summary>
        ///     The IProgress to use to report the progress of the extractor. <br />
        ///     The first report that is sent contains the total amount of files to extract.
        /// </summary>
        public IProgress<int> ProgressReporter { get; }

        /// <summary>
        ///     Executes the request.
        /// </summary>
        public virtual void ExecuteRequest()
        {
            using (FileStream stream = FileIO.Open(FilePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                File = new ModFile(reader);
                File.PopulateFiles();
            }

            ExtractAllFiles(File.Files, ExtractDirectory);
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

            // Report the total amount of files to the progress reporter
            ProgressReporter.Report(files.Count);

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

                // Report that a file has been completed to the progress reporter
                ProgressReporter.Report(1);
            }
        }
    }
}