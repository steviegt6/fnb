using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TML.Files;
using TML.Files.Utilities;

namespace TML.Patcher.Tasks
{
    /// <summary>
    ///     Class for repacking files into a .tmod file.
    /// </summary>
    public class RepackTask : ProgressTask
    {
        /// <summary>
        ///     Bytes for "TMOD", the expected file header.
        /// </summary>
        public const string ModFileHeader = "TMOD";

        /// <summary>
        ///     Constructs a new <see cref="RepackTask"/> instance.
        /// </summary>
        public RepackTask(DirectoryInfo repackDirectory, string targetFilePath, ModData modData, double threads)
        {
            RepackDirectory = repackDirectory;
            TargetFilePath = targetFilePath;
            ModData = modData;
            Threads = threads;
        }

        /// <summary>
        ///     The repacking directory..
        /// </summary>
        public DirectoryInfo RepackDirectory { get; }

        /// <summary>
        ///     Target file output path.
        /// </summary>
        public string TargetFilePath { get; }

        /// <summary>
        ///     Mod data used for writing.
        /// </summary>
        public ModData ModData { get; }

        /// <summary>
        ///     The amount of threads to use.
        /// </summary>
        public double Threads { get; private set; }

        /// <summary>
        ///     Executes the repackaging request.
        /// </summary>
        public override async Task ExecuteAsync()
        {
            ConcurrentBag<FileEntryData> entries = ConvertFilesToEntries();
            ConvertToModFile(entries);

            await Task.CompletedTask;
        }

        /// <summary>
        ///     Converts a collection of <see cref="FileEntryData"/>s to a mod file.
        /// </summary>
        /// <param name="entriesEnumerable"></param>
        protected virtual void ConvertToModFile(IEnumerable<FileEntryData> entriesEnumerable)
        {
            ProgressReporter.Report("Writing mundane TMOD information.");
            
            // Convert entries IEnumerable to an array
            FileEntryData[] entries = entriesEnumerable.ToArray();

            FileStream modStream = new(TargetFilePath, FileMode.Create);
            BinaryWriter modWriter = new(modStream);

            // Write the header
            modWriter.Write(Encoding.UTF8.GetBytes(ModFileHeader));

            // Write the mod loader version
            modWriter.Write(ModData.ModLoaderVersion.ToString());

            // Store the position of the hash
            long hashPos = modStream.Position;

            // Skip 20 + 256 + 4 bytes of the hash, browser signature, and file data length
            modStream.Seek(20 + 256 + 4, SeekOrigin.Current);

            // Store the position of the start of the mod's data
            long dataPos = modStream.Position;

            // Write the mod's internal name
            modWriter.Write(ModData.ModName);

            // Write the mod's version
            modWriter.Write(ModData.ModVersion.ToString());

            // Write the number of files in the .tmod file
            modWriter.Write(entries.Length);

            // Iterate over all entries and write:
            // * File path
            // * Uncompressed length
            // * Compressed length
            for (int i = 0; i < entries.Length; i++)
            {
                ProgressReporter.Report(
                    new ProgressNotification("Writing file entry data", entries.Length, i)
                );
                
                FileEntryData entry = entries[i];
                modWriter.Write(entry.FileName);
                modWriter.Write(entry.FileLengthData.Length);
                modWriter.Write(entry.FileLengthData.LengthCompressed);
            }

            // Iterate over all entries and write the entry bytes
            for (int i = 0; i < entries.Length; i++)
            {
                ProgressReporter.Report(
                    new ProgressNotification("Writing file entry bytes", entries.Length, i)
                );
                
                FileEntryData entry = entries[i];
                modWriter.Write(entry.FileData);
            }

            ProgressReporter.Report(
                "Finishing writing mundane TMOD information"
            );

            // Go to the start of the mod's data to calculate the hash
            modStream.Position = dataPos;
            byte[] hash = SHA1.Create().ComputeHash(modStream);

            // Go to the hash position to write the hash
            modStream.Position = hashPos;
            modWriter.Write(hash);

            // Skip the mod browser signature
            modStream.Seek(256, SeekOrigin.Current);

            // Calculate the bytes of the mod's data and write it
            int modBytes = (int) (modStream.Length - dataPos);
            modWriter.Write(modBytes);

            // Close the file
            modStream.Dispose();
        }

        /// <summary>
        ///     Converts <see cref="FileInfo"/> instances to <see cref="FileEntryData"/> instances.
        /// </summary>
        /// <returns></returns>
        protected virtual ConcurrentBag<FileEntryData> ConvertFilesToEntries()
        {
            ProgressReporter.Report("Collecting files to repack.");
            
            List<FileInfo> files = new(RepackDirectory.GetFiles("*", SearchOption.AllDirectories));
            List<List<FileInfo>> chunks = new();
            ConcurrentBag<FileEntryData> bag = new();

            if (Threads <= 0D)
                Threads = 1D;

            // use either the amount of configured threads or the amount of files (whichever is lower)
            double numThreads = Math.Min(files.Count, Threads);
            int chunkSize = (int) Math.Round(files.Count / numThreads, MidpointRounding.AwayFromZero);

            // Split the files into chunks
            for (int i = 0; i < files.Count; i += chunkSize)
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));
            
            ProgressReporter.Report($"Processing \"{chunks.Count}\" chunk(s).");

            // Run a task for each chunk
            // Wait for all tasks to finish
            Task.WaitAll(chunks.Select(chunk =>
                Task.Run(() => ConvertChunkToEntry(chunk, bag, RepackDirectory.FullName))
            ).ToArray());

            return bag;
        }

        private static void ConvertChunkToEntry(IEnumerable<FileInfo> chunk, ConcurrentBag<FileEntryData> fileBag,
            string baseFolder)
        {
            foreach (FileInfo file in chunk)
            {
                int newLengthCompressed;
                byte[] newFileData;

                FileStream stream = file.OpenRead();
                MemoryStream memStream = new();
                stream.CopyTo(memStream);

                // Set the uncompressed length of the file
                int newLength = (int) stream.Length;

                // Check if the file is bigger than 1KB, and if it is, compress it
                // TODO: Convert compress required size to an option
                if (stream.Length > 1024 && ShouldCompress(file.Extension))
                {
                    byte[] compressedStream = FileUtilities.CompressFile(memStream.ToArray());
                    newLengthCompressed = compressedStream.Length;
                    newFileData = compressedStream;
                }
                else
                {
                    newLengthCompressed = newLength;
                    newFileData = memStream.ToArray();
                }

                // Set the file name of the entry and the length data
                string newFileName = Path.GetRelativePath(baseFolder, file.FullName).Replace('\\', '/');
                FileLengthData newFileLengthData = new(newLength, newLengthCompressed);

                // Add the entry to the concurrent bag
                fileBag.Add(new FileEntryData(newFileName, newFileLengthData, newFileData));
            }
        }

        public static bool ShouldCompress(string extension) => extension != ".png" &&
                                                                extension != ".rawimg" &&
                                                                extension != ".ogg" &&
                                                                extension != ".mp3";
    }
}