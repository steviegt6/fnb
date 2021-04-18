#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Consolation.Common.Framework.OptionsSystem;
using TML.Files.Generic.Data;
using TML.Files.Generic.Files;
using TML.Files.Specific.Files;

namespace TML.Patcher.Common.Options
{
    public class UnpackModOption : ConsoleOption
    {
        public override string Text => "Unpack a mod.";

        public override void Execute()
        {
            string modName = GetModName();
            
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

        private static string GetModName()
        {
            while (true)
            {
                Console.WriteLine("Please enter the name of the mod you want to extract:");
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    Program.Instance.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (File.Exists(Path.Combine(Program.Configuration.ModsPath, modName))) 
                    return modName;
                
                Program.Instance.WriteAndClear("Specified mod could not be located!");
            }
        }

        private static void ExtractAllFiles(List<FileEntryData> files, DirectoryInfo extractDirectory)
        {
            List<Task> tasks = new();
            List<List<FileEntryData>> chunks = new();

            // TODO: Add an option for the number of tasks to use
            double numThreads = Math.Min(files.Count, 4); // Use either '4' threads, or the number of files, whatever is lower
            int chunkSize = (int) Math.Round(files.Count / numThreads, MidpointRounding.AwayFromZero);

            // Split the files into chunks
            for (int i = 0; i < files.Count; i += chunkSize)
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));

            // Run a task for each chunk
            foreach (List<FileEntryData> chunk in chunks)
                tasks.Add(Task.Run(() => ExtractChunkFiles(chunk, extractDirectory)));

            // Wait for all tasks to finish
            Task.WaitAll(tasks.ToArray());
        }

        private static void ExtractChunkFiles(IEnumerable<FileEntryData> files, DirectoryInfo extractDirectory)
        {
            foreach (FileEntryData file in files)
            {
                Console.WriteLine($" Extracting file: {file.fileName}");

                byte[] data = file.fileData;

                if (file.fileLengthData.length != file.fileLengthData.lengthCompressed)
                    data = Decompress(file.fileData);

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

        private static byte[] Decompress(byte[] data)
        {
            MemoryStream dataStream = new(data);
            MemoryStream emptyStream = new();
            using (DeflateStream deflatedStream = new(dataStream, CompressionMode.Decompress))
            {
                deflatedStream.CopyTo(emptyStream);
            }
            
            return emptyStream.ToArray();
        }
        
        private static void ConvertRawToPng(byte[] data, string properPath)
        {
            using MemoryStream input = new(data);
            using BinaryReader reader = new(input);
            reader.ReadInt32();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            ImagePixelColor[] colors = new ImagePixelColor[width * height];

            for (int i = 0; i < colors.Length; i++)
                colors[i] = new ImagePixelColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

            Bitmap imageMap = new(width, height, PixelFormat.Format32bppArgb);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                ImagePixelColor pixelColor = colors[y * width + x];
                imageMap.SetPixel(x, y, Color.FromArgb(pixelColor.a, pixelColor.r, pixelColor.g, pixelColor.b));
            }

            imageMap.Save(Path.ChangeExtension(properPath, ".png"));
        }
    }
}
