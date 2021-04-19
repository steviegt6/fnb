#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Consolation.Common.Framework.OptionsSystem;
using TML.Files.Generic.Files;
using TML.Files.Specific.Files;

namespace TML.Patcher.Common.Options
{
    public class UnpackModOption : ConsoleOption
    {
        public override string Text => "Unpack a mod.";

        public override void Execute()
        {
            string modName = GetModName(Program.Configuration.ModsPath);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine( $" Extracting mod: {modName}...");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            
            DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(Program.Configuration.ExtractPath, modName));
            TModFile modFile;
            using (FileStream stream = File.Open(Path.Combine(Program.Configuration.ModsPath, modName), FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                modFile = new TModFile(reader);
            }
            
            Stopwatch sw = Stopwatch.StartNew();
            ExtractAllFiles(modFile.files, directory);
            sw.Stop();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" Finished extracting mod: {modName}");
            Console.WriteLine($" Extraction time: {sw.Elapsed}");

            Program.Instance.WriteOptionsList(new ConsoleOptions("Return:"));
        }

        private static string GetModName(string pathToSearch)
        {
            while (true)
            {
                Program.Instance.WriteAndClear("Please enter the name of the mod you want to extract:", ConsoleColor.Yellow);
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    Program.Instance.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (File.Exists(Path.Combine(pathToSearch, modName))) 
                    return modName;
                
                Program.Instance.WriteAndClear("Specified mod could not be located!");
            }
        }

        private static void ExtractAllFiles(List<FileEntryData> files, DirectoryInfo extractDirectory)
        {
            List<List<FileEntryData>> chunks = new();

            if (Program.Configuration.Threads <= 0)
                Program.Configuration.Threads = 1;

            double numThreads = Math.Min(files.Count, Program.Configuration.Threads); // Use either '4' threads, or the number of files, whatever is lower
            int chunkSize = (int) Math.Round(files.Count / numThreads, MidpointRounding.AwayFromZero);

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
                // Console.WriteLine($" Extracting file: {file.fileName}");

                byte[] data = file.fileData;

                if (file.fileLengthData.length != file.fileLengthData.lengthCompressed)
                    data = Decompress(file.fileData, file.fileLengthData.length);

                string[] pathParts = file.fileName.Split(Path.DirectorySeparatorChar);
                string[] mendedPath = new string[pathParts.Length + 1];
                mendedPath[0] = extractDirectory.FullName;

                for (int i = 0; i < pathParts.Length; i++)
                    mendedPath[i + 1] = pathParts[i];

                string properPath = Path.Combine(mendedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(properPath) ?? string.Empty);

                if (Path.GetExtension(properPath) == ".rawimg")
                    ConvertRawToPng(data, properPath);
                else
                    File.WriteAllBytes(properPath, data);
            }
        }

        private static byte[] Decompress(byte[] data, int decompressedSize)
        {
            MemoryStream dataStream = new(data);
            byte[] decompressed = new byte[decompressedSize];
            
            using DeflateStream deflatedStream = new(dataStream, CompressionMode.Decompress);
            deflatedStream.Read(decompressed, 0, decompressedSize);
            
            return decompressed;
        }
        
        private static unsafe void ConvertRawToPng(byte[] data, string properPath)
        {
            using MemoryStream input = new(data);
            using BinaryReader reader = new(input);
            reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            byte[] oldPixels = reader.ReadBytes(width * height * 4);

            Bitmap imageMap = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData bitmapData = imageMap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, imageMap.PixelFormat);

            Parallel.For(0, bitmapData.Height, y =>
            {
                int currentLine = y * bitmapData.Stride;
                byte* row = (byte*) bitmapData.Scan0 + currentLine;
                for (int x = 0; x < bitmapData.Width; x++)
                {
                    int posRaw = x * 4;
                    int posNormal = posRaw + currentLine;
                    
                    row[posRaw + 2] = oldPixels[posNormal + 0]; // R
                    row[posRaw + 1] = oldPixels[posNormal + 1]; // G
                    row[posRaw + 0] = oldPixels[posNormal + 2]; // B
                    row[posRaw + 3] = oldPixels[posNormal + 3]; // A
                }
            });
            
            imageMap.UnlockBits(bitmapData);
            
            imageMap.Save(Path.ChangeExtension(properPath, ".png"));
        }
    }
}
