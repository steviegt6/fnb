using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TML.Files.Generic.Files;
using TML.Files.Generic.Utilities;
using TML.Files.Specific.Files;

namespace TML.Patcher.Backend.Packing
{
    public sealed class RepackRequest
    {
        private const string ModFileHeader = "TMOD"; // Bytes for TMOD
        
        public RepackRequest(DirectoryInfo repackDirectory, string targetFilePath, ModData modData, double threads)
        {
            RepackDirectory = repackDirectory;
            TargetFilePath = targetFilePath;
            ModData = modData;
            Threads = threads;
        }

        public DirectoryInfo RepackDirectory { get; }

        public string TargetFilePath { get; }
        
        public ModData ModData { get; }
        
        public double Threads { get; private set;  }

        public void ExecuteRequest()
        {
            ConcurrentBag<FileEntryData> entries = ConvertFilesToEntries();
            ConvertToModFile(entries);
        }

        private void ConvertToModFile(IEnumerable<FileEntryData> entriesEnumerable)
        {
            // Convert entries IEnumerable to an array
            FileEntryData[] entries = entriesEnumerable.ToArray();
            
            FileStream modStream = new(TargetFilePath, FileMode.Create);
            BinaryWriter modWriter = new(modStream);
            
            // Write the header
            modWriter.Write(Encoding.UTF8.GetBytes(ModFileHeader));
            
            // Write the mod loader version
            modWriter.Write(ModData.modLoaderVersion.ToString());
            
            // Store the position of the hash
            long hashPos = modStream.Position;
            
            // Skip 20 + 256 + 4 bytes of the hash, browser signature, and file data length
            modStream.Seek(20 + 256 + 4, SeekOrigin.Current);
            
            // Store the position of the start of the mod's data
            long dataPos = modStream.Position;
            
            // Write the mod's internal name
            modWriter.Write(ModData.modName);
            
            // Write the mod's version
            modWriter.Write(ModData.modVersion.ToString());
            
            // Write the number of files in the .tmod file
            modWriter.Write(entries.Length);
            
            // Iterate over all entries and write:
            // * File path
            // * Uncompressed length
            // * Compressed length
            foreach (FileEntryData entry in entries)
            {
                modWriter.Write(entry.fileName);
                modWriter.Write(entry.fileLengthData.length);
                modWriter.Write(entry.fileLengthData.lengthCompressed);
            }
            
            // Iterate over all entries and write the entry bytes
            foreach (FileEntryData entry in entries)
                modWriter.Write(entry.fileData);
            
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

        private ConcurrentBag<FileEntryData> ConvertFilesToEntries()
        {
            List<FileInfo> files = new(RepackDirectory.GetFiles("*", SearchOption.AllDirectories));
            
            List<List<FileInfo>> chunks = new();

            ConcurrentBag<FileEntryData> fileBag = new();

            if (Threads <= 0)
                Threads = 1D;

            double numThreads = 
                Math.Min(files.Count, Threads); // use either the amount of configured threads or the amount of files (whichever is lower)
            int chunkSize = (int) Math.Round(files.Count / numThreads, MidpointRounding.AwayFromZero);

            // Split the files into chunks
            for (int i = 0; i < files.Count; i += chunkSize)
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));

            // Run a task for each chunk
            // Wait for all tasks to finish
            Task.WaitAll(chunks.Select(chunk => Task.Run(() => ConvertChunkToEntry(chunk, fileBag, RepackDirectory.FullName))).ToArray());

            return fileBag;
        }

        private static void ConvertChunkToEntry(IEnumerable<FileInfo> chunk, ConcurrentBag<FileEntryData> fileBag, string baseFolder)
        {
            foreach (FileInfo file in chunk)
            {
                FileEntryData entryData = new();
                FileLengthData lengthData = new();

                FileStream fileStream = file.OpenRead();
                MemoryStream fileMemStream = new();
                fileStream.CopyTo(fileMemStream);

                // Set the uncompressed length of the file
                lengthData.length = (int) fileStream.Length;
                
                // Check if the file is bigger than 1KB, and if it is, compress it
                // TODO: Convert compress required size to an option
                if (fileStream.Length > 1024 && ShouldCompress(file.Extension))
                {
                    byte[] compressedStream = FileUtilities.CompressFile(fileMemStream.ToArray());
                    lengthData.lengthCompressed = compressedStream.Length;
                    entryData.fileData = compressedStream;
                }
                else
                {
                    lengthData.lengthCompressed = lengthData.length;
                    entryData.fileData = fileMemStream.ToArray();
                }

                // Set the file name of the entry and the length data
                entryData.fileName = Path.GetRelativePath(baseFolder, file.FullName).Replace('\\', '/');
                entryData.fileLengthData = lengthData;
                
                // Add the entry to the concurrent bag
                fileBag.Add(entryData);
            }
        }

        private static bool ShouldCompress(string extension) =>
            extension != ".png" && extension != ".rawimg" && extension != ".ogg" && extension != ".mp3";
    }
}