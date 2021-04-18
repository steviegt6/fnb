#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using TML.Files.Generic.Data;
using TML.Files.Generic.Files;
using TML.Files.Specific.Data;
using TML.Files.Specific.Files;
using TML.Patcher.Common.Framework;

namespace TML.Patcher.Common.Options
{
    public class UnpackModOption : ConsoleOption
    {
        private bool _extractionInProcess;
        private bool _threadCompletedTask;
        private readonly ConcurrentBag<(FileEntryData, byte[], string)> _filesToConvert = new();

        public override string Text => "Unpack a mod.";

        public override void Execute()
        {
            while (true)
            {
                _extractionInProcess = true;
                _threadCompletedTask = false;

                Console.WriteLine("Please enter the name of the mod you want to extract:");
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    Program.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (!File.Exists(Path.Combine(Program.Configuration.ModsPath, modName)))
                {
                    Program.WriteAndClear("Specified mod could not be located!");
                    continue;
                }

                Stopwatch timeTook = new();
                timeTook.Start();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Starting image conversion thread...");
                new Thread(ConvertAllRawsToPNGs).Start();
                Console.WriteLine("Started image conversion thread...");

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



                foreach (FileEntryData file in modFile.files)
                {
                    Console.WriteLine($" Extracting file: {file.fileName}");

                    byte[] data = file.fileData;

                    if (file.fileLengthData.length != file.fileLengthData.lengthCompressed)
                        data = Decompress(file.fileData);

                    string[] pathParts = file.fileName.Split(Path.DirectorySeparatorChar);
                    string[] mendedPath = new string[pathParts.Length + 1];
                    mendedPath[0] = directory.FullName;

                    for (int i = 0; i < pathParts.Length; i++)
                        mendedPath[i + 1] = pathParts[i];

                    string properPath = Path.Combine(mendedPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(properPath) ?? string.Empty);

                    if (Path.GetExtension(properPath) == ".rawimg")
                        _filesToConvert.Add((file, data, properPath));
                    else
                        File.WriteAllBytes(properPath, data);
                }

                timeTook.Stop();

                while (!_threadCompletedTask) 
                    Thread.Sleep(100);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" Finished extracting mod: {modName}");
                Console.WriteLine($" Extraction finished in: {timeTook.Elapsed}");
                // WriteBuildFile(SaveInfoAsBuild(directory), directory);
                break;
            }

            _extractionInProcess = false;
            _threadCompletedTask = false;
            _filesToConvert.Clear();
            Program.WriteOptionsList(new ConsoleOptions("Return:"));
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

        private void ConvertAllRawsToPNGs()
        {
            List<string> pathsConverted = new();

            while (_extractionInProcess)
            {
                if (pathsConverted.Count == _filesToConvert.Count && _filesToConvert.Count > 0)
                {
                    _threadCompletedTask = true;
                    break;
                }

                foreach ((FileEntryData file, byte[] data, string path) in _filesToConvert)
                {
                    if (pathsConverted.Contains(path))
                        continue;

                    Console.WriteLine($" Converting {file.fileName} to .png");
                    SaveRawToPNG(data, path);
                    pathsConverted.Add(path);
                }
            }
        }

        private static void SaveRawToPNG(byte[] data, string properPath)
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
