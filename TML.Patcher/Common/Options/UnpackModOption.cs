#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TML.Files.Generic;
using TML.Files.Specific;
using TML.Patcher.Common.Framework;

// Modified tModViewer code
// Thanks, Trivaxy!
namespace TML.Patcher.Common.Options
{
    public class UnpackModOption : ConsoleOption
    {
        public readonly struct ImagePixelColor
        {
            public readonly int r;
            public readonly int g;
            public readonly int b;
            public readonly int a;

            public ImagePixelColor(int r, int g, int b, int a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
        }

        public struct BuildProperties
        {
            public string[] dllReferences;
            public string[] modReferences;
            public string[] weakReferences;
            public string[] sortAfter;
            public string[] sortBefore;
            public string[] buildIgnores;
            public string author;
            public Version version;
            public string displayName;
            public string homepage;
            public string description;
            public bool noCompile;
            public bool hideCode;
            public bool hideResources;
            public bool includeSource;
            public bool includePDB;
            public ModSide side;

            // hidden
            public string eacPath;
            public bool beta;
            public Version buildVersion;

            public BuildProperties(bool beta)
            {
                this.beta = beta;
                side = ModSide.Client;
                dllReferences = Array.Empty<string>();
                modReferences = Array.Empty<string>();
                weakReferences = Array.Empty<string>();
                sortAfter = Array.Empty<string>();
                sortBefore = Array.Empty<string>();
                buildIgnores = Array.Empty<string>();
                author = "noauthor";
                version = new Version(0, 0, 0, 1);
                displayName = "nodisplayname";
                homepage = "nohomepage";
                description = "nodesc";
                noCompile = false;
                hideCode = false;
                hideResources = false;
                includeSource = false;
                includePDB = false;
                eacPath = "noeacpath";
                buildVersion = new Version(0, 0, 0, 1);
            }
        }

        public enum ModSide
        {
            Both,
            Client,
            Server,
            NoSync
        }

        public override string Text => "Unpack a mod.";

        public override void Execute()
        {
            while (true)
            {
                Console.WriteLine("Please enter the name of the mod you want to extract:");
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    Program.WriteAndClear("Specified mod name somehow returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (!File.Exists(Path.Combine(Program.Configuration.ModsPath, modName)))
                {
                    Program.WriteAndClear("Specified mod could not be located!");
                    continue;
                }

                DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(Path.Combine(Program.EXEPath, "Extracted"), modName));
                TModFile modFile;
                using (FileStream stream = File.Open(Path.Combine(Program.Configuration.ModsPath, modName), FileMode.Open))
                using (BinaryReader reader = new(stream))
                {
                    modFile = new TModFile(reader);
                }

                foreach (FileEntryData file in modFile.files)
                {
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
                        SaveRawToPNG(data, properPath);
                    else
                        File.WriteAllBytes(properPath, data);
                }

                WriteBuildFile(SaveInfoAsBuild(directory), directory);

                break;
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
