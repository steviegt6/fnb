using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TML.Files;
using TML.Files.Utilities;

namespace TML.Patcher.Tasks
{
    public class UnpackTask : ProgressTask
    {
        public DirectoryInfo ExtractDirectory { get; }

        public string FilePath { get; }

        /// <summary>
        ///     Amount of threads to use.
        /// </summary>
        public double Threads { get; set; }
        
        public UnpackTask(DirectoryInfo extractDirectory, string path, double threads)
        {
            ExtractDirectory = extractDirectory;
            FilePath = path;
            Threads = threads;
        }

        public override Task ExecuteAsync()
        {
            ProgressReporter.Report("Reading the .tmod file.");
            
            using ModFile modFile = new(FilePath);

            ExtractAllFiles(modFile, ExtractDirectory);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Extracts all <see cref="FileEntryData"/> instances.
        /// </summary>
        protected virtual void ExtractAllFiles(ModFile modFile, FileSystemInfo extractDirectory)
        {
            List<ModFileEntry> fileList = modFile.FileEntries.ToList();
            List<List<ModFileEntry>> chunks = new();

            if (Threads <= 0)
                Threads = 1D;

            // use either the amount of configured threads or the amount of files (whichever is lower)
            double numThreads = Math.Min(fileList.Count, Threads);
            int chunkSize = (int) Math.Round(fileList.Count / numThreads, MidpointRounding.AwayFromZero);

            // Split the files into chunks
            for (int i = 0; i < fileList.Count; i += chunkSize)
                chunks.Add(fileList.GetRange(i, Math.Min(chunkSize, fileList.Count - i)));
            
            ProgressReporter.Report($"Processing {chunks.Count} chunk(s).");

            // Run a task for each chunk
            // Wait for all tasks to finish
            Task.WaitAll(chunks.Select(chunk => Task.Run(() => ExtractChunkFiles(chunk, extractDirectory))).ToArray());
        }

        /// <summary>
        ///     Extracts a chunk of given files. Used for multi-threading.
        /// </summary>
        protected virtual void ExtractChunkFiles(IEnumerable<ModFileEntry> files, FileSystemInfo extractDirectory)
        {
            foreach (ModFileEntry file in files)
            {
                byte[] data = file.CachedBytes ?? Array.Empty<byte>();

                if (file.IsCompressed)
                    data = FileUtilities.DecompressFile(data);

                string[] pathParts = file.Name.Split(Path.DirectorySeparatorChar);
                string[] mendedPath = new string[pathParts.Length + 1];
                mendedPath[0] = extractDirectory.FullName;

                for (int i = 0; i < pathParts.Length; i++)
                    mendedPath[i + 1] = pathParts[i];

                string properPath = Path.Combine(mendedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(properPath) ?? string.Empty);

                if (file.Name == "Info") {
                    ExtractInfoFile(properPath, data);
                    continue;
                }
                
                if (Path.GetExtension(properPath) == ".rawimg")
                    FileConversion.ConvertRawToPng(data, properPath);
                else
                    File.WriteAllBytes(properPath, data);
            }
        }
        
        private static IEnumerable<string> ReadList(BinaryReader reader) {
            List<string> list = new();
            for (string item = reader.ReadString(); item.Length > 0; item = reader.ReadString())
                list.Add(item);

            return list;
        }

        protected virtual void ExtractInfoFile(string properPath, byte[] data) {
            StringBuilder sb = new();

            using MemoryStream memStream = new(data);
            using BinaryReader reader = new(memStream);

            // 'While the intended defaults for these are false, Info will only have !hideCode and !hideResources entries, so this is necessary.'
            bool hideCode = true, hideResources = true;
            
            for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString()) {
                string? value = null;
                switch (tag) {
                    case "dllReferences" or "modReferences" or "weakReferences" or "sortAfter" or "sortBefore":
                        value = string.Join(", ", ReadList(reader));
                        break;
                    case "noCompile" or "includeSource" or "includePDB" or "beta":
                        value = "true";
                        break;
                    case "!hideCode":
                        hideCode = false;
                        break;
                    case "!hideResources":
                        hideResources = false;
                        break;
                    case "side":
                        value = reader.ReadByte() switch {
                            0 => "Both",
                            1 => "Client",
                            2 => "Server",
                            3 => "NoSync",
                            _ => value,
                        };
                        break;
                    case "description":
                        continue;
                    default:
                        value = reader.ReadString();
                        break;
                }
                
                if (value is not null)
                    sb.AppendLine($"{tag} = {value}");
            }

            if (hideCode)
                sb.AppendLine("hideCode = true");
            if (hideResources)
                sb.AppendLine("hideResources = true");

            string parentPath = Path.GetDirectoryName(properPath)!;
            string buildPath = Path.Combine(parentPath, "build.txt");
            
            File.WriteAllText(buildPath, sb.ToString());
        }
    }
}