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
using TML.Files.Generic.Data;
using TML.Files.Generic.Files;
using TML.Files.Specific.Data;
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
            
            var sw = Stopwatch.StartNew();
            ExtractAllFiles(modFile.files, directory);
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" Finished extracting mod: {modName}");
            Console.WriteLine($"Took {elapsed} ms");

            Program.Instance.WriteOptionsList(new ConsoleOptions("Return:"));
        }

        private string GetModName()
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

        private void ExtractAllFiles(List<FileEntryData> files, DirectoryInfo extractDirectory)
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

        private void ExtractChunkFiles(List<FileEntryData> files, DirectoryInfo extractDirectory)
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
                    ConvertRawToPng(file, data, properPath);
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
        
        private void ConvertRawToPng(FileEntryData file, byte[] data, string properPath)
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

        private static BuildProperties SaveInfoAsBuild(FileSystemInfo directory)
        {
            string modPath = directory.FullName;
            string buildPath = Path.Combine(modPath, "Info");
            string descPath = Path.Combine(modPath, "description.txt");
            BuildProperties properties = new(false);

            if (!File.Exists(buildPath))
                return properties;

            if (File.Exists(descPath))
                properties.description = File.ReadAllText(descPath);

            foreach (string line in File.ReadAllLines(buildPath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                int split = line.IndexOf('=');

                if (split == -1)
                    continue;

                string property = line.Substring(0, split).Trim();
                string value = line.Substring(split + 1).Trim();

                if (value.Length == 0)
                    continue;

                switch (property)
                {
                    case "dllReferences":
                        properties.dllReferences = ReadList(value).ToArray();
                        break;

                    case "modReferences":
                        properties.modReferences = ReadList(value).ToArray();
                        break;

                    case "weakReferences":
                        properties.weakReferences = ReadList(value).ToArray();
                        break;

                    case "sortBefore":
                        properties.sortBefore = ReadList(value).ToArray();
                        break;

                    case "sortAfter":
                        properties.sortAfter = ReadList(value).ToArray();
                        break;

                    case "author":
                        properties.author = value;
                        break;

                    case "version":
                        properties.version = new Version(value);
                        break;

                    case "displayName":
                        properties.displayName = value;
                        break;

                    case "homepage":
                        properties.homepage = value;
                        break;

                    case "noCompile":
                        properties.noCompile = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case "hideCode":
                        properties.hideCode = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case "hideResources":
                        properties.hideResources = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case "includeSource":
                        properties.includeSource = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case "includePDB":
                        properties.includePDB = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case "buildIgnore":
                        properties.buildIgnores = value.Split(',').Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)).Where(s => s.Length > 0).ToArray();
                        break;

                    case "side":
                        if (!Enum.TryParse(value, true, out properties.side))
                            throw new Exception("side is not one of (Both, Client, Server, NoSync): " + value);
                        break;
                }
            }

            return properties;
        }

        private static void WriteBuildFile(BuildProperties properties, FileSystemInfo directory)
        {
            string modPath = directory.FullName;
            string propertiesPath = Path.Combine(modPath, "Info");
            string buildPath = Path.Combine(modPath, "build.txt");

            // File.Delete(propertiesPath); // get rid of info file since we no longer need it

            using MemoryStream input = new();
            using BinaryWriter writer = new(input);
            if (properties.dllReferences.Length > 0)
            {
                writer.Write("dllReferences");
                WriteList(properties.dllReferences, writer);
            }

            if (properties.modReferences.Length > 0)
            {
                writer.Write("modReferences");
                WriteList(properties.modReferences, writer);
            }

            if (properties.weakReferences.Length > 0)
            {
                writer.Write("weakReferences");
                WriteList(properties.weakReferences, writer);
            }

            if (properties.sortAfter.Length > 0)
            {
                writer.Write("sortAfter");
                WriteList(properties.sortAfter, writer);
            }

            if (properties.sortBefore.Length > 0)
            {
                writer.Write("sortBefore");
                WriteList(properties.sortBefore, writer);
            }

            if (properties.author.Length > 0)
            {
                writer.Write("author");
                writer.Write(properties.author);
            }

            writer.Write("version");
            writer.Write(properties.version.ToString());

            if (properties.displayName.Length > 0)
            {
                writer.Write("displayName");
                writer.Write(properties.displayName);
            }

            if (properties.homepage.Length > 0)
            {
                writer.Write("homepage");
                writer.Write(properties.homepage);
            }

            if (properties.description.Length > 0)
            {
                writer.Write("description");
                writer.Write(properties.description);
            }

            if (properties.noCompile)
            {
                writer.Write("noCompile");
            }

            if (!properties.hideCode)
            {
                writer.Write("!hideCode");
            }

            if (!properties.hideResources)
            {
                writer.Write("!hideResources");
            }

            if (properties.includeSource)
            {
                writer.Write("includeSource");
            }

            if (properties.includePDB)
            {
                writer.Write("includePDB");
            }

            if (properties.eacPath.Length > 0)
            {
                writer.Write("eacPath");
                writer.Write(properties.eacPath);
            }

            if (properties.side != ModSide.Both)
            {
                writer.Write("side");
                writer.Write((byte)properties.side);
            }

            /*if (ModLoader.beta > 0)
                {
                    writer.Write("beta");
                }*/

            writer.Write("buildVersion");
            writer.Write(properties.buildVersion.ToString());

            writer.Write("");

            File.WriteAllBytes(buildPath, input.ToArray());
        }

        private static IEnumerable<string> ReadList(string value) => value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);

        private static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer)
        {
            foreach (T item in list)
                writer.Write(item?.ToString() ?? string.Empty);

            writer.Write("");
        }
    }
}
