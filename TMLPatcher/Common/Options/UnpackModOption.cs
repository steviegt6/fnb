using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using TMLPatcher.Common.Framework;
using TMLPatcher.Common.TML;

// Modified tModViewer code
// Thanks, Trivaxy!
namespace TMLPatcher.Common.Options
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

        public override string Text => "Unpack a mod.";

        public override void Execute()
        {
            while (true)
            {
                Console.WriteLine("Please enter the name of the mod you want to extract:");
                string modName = Console.ReadLine();

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

                foreach (TModFileEntry file in modFile.files)
                {
                    byte[] data = file.fileData;

                    if (file.fileLength.length != file.fileLength.lengthCompressed)
                        data = Decompress(file.fileData);

                    string[] pathParts = file.fileName.Split(Path.DirectorySeparatorChar);
                    string[] mendedPath = new string[pathParts.Length + 1];
                    mendedPath[0] = directory.FullName;

                    for (int i = 0; i < pathParts.Length; i++)
                        mendedPath[i + 1] = pathParts[i];

                    string properPath = Path.Combine(mendedPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(properPath) ?? string.Empty);

                    if (Path.GetExtension(properPath) == ".rawimg")
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
                    else
                        File.WriteAllBytes(properPath, data);
                }

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
    }
}
