using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Graphics;
using TMLPatcher.Common.Framework;
using TMLPatcher.Common.TML;

// https://github.com/tModLoader/tModLoader/blob/master/patches/tModLoader/Terraria.ModLoader.UI/UIExtractMod.cs
namespace TMLPatcher.Common.Options
{
    public class UnpackModOption : ConsoleOption
    {
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
                        MemoryStream memoryStream = new();

                        using (MemoryStream input = new(data))
                        {
                            using (BinaryReader reader = new(input))
                            {
                                reader.ReadInt32();
                                int width = reader.ReadInt32();
                                int height = reader.ReadInt32();
                                byte[] texData = reader.ReadBytes(width * height * 4);
                                Texture2D tex = new(new GraphicsDevice(null, GraphicsProfile.HiDef, new PresentationParameters()), width, height);
                                // TODO: images
                            }
                        }
                        File.WriteAllBytes(Path.ChangeExtension(properPath, ".png"), memoryStream.ToArray());
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
