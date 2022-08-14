using System.Collections.Generic;
using System.IO;
using System.Text;
using TML.Files.Abstractions;

namespace TML.Files.Extractors
{
    public class InfoFileExtractor : IFileExtractor
    {
        public bool ShouldExtract(IModFileEntry fileEntry) {
            // build.txt is turned into the "Info" file in the root directory.
            return fileEntry.Name == "Info";
        }

        public IExtractedModFile Extract(IModFileEntry fileEntry, byte[] data) {
            StringBuilder sb = new();
            using MemoryStream stream = new(data);
            using BinaryReader reader = new(stream);

            for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString()) {
                string? value = null;
                switch (tag) {
                    case "dlLReferences" or "modReferences" or "weakReferences" or "sortAfter" or "sortBefore":
                        value = string.Join(", ", ReadList(reader));
                        break;

                    case "noCompile" or "includeSource" or "includePDB" or "beta":
                        value = "true";
                        break;

                    case "!hideCode" or "!hideResources" or "!playableOnPreview":
                        value = "false";
                        tag = tag[1..];
                        break;

                    case "side":
                        value = reader.ReadByte() switch
                        {
                            0 => "Both",
                            1 => "Client",
                            2 => "Server",
                            3 => "NoSync",
                            _ => null,
                        };
                        break;

                    // Junk data, already saved to description.txt anyway.
                    case "description":
                        break;

                    // TODO: eacPath and buildVersion aren't properties set by the user. Should we still include these?
                    // case "eacPath":
                    // case "buildVersion":
                    case "displayName":
                    case "author":
                    case "version":
                    case "homepage":
                    case "languageVersion":
                        value = reader.ReadString();
                        break;
                }

                if (value is not null) sb.AppendLine($"{tag} = {value}");
            }

            string? dirName = Path.GetDirectoryName(fileEntry.Name);
            string path = dirName is not null ? Path.Combine(dirName, "build.txt") : "build.txt";

            // Encoded in UTF-8. Kinda cringe.
            // https://github.com/tModLoader/tModLoader/blob/e88e8677f419f9bf7b268cec1e2d3e1b62ea63aa/patches/tModLoader/Terraria/ModLoader/Core/BuildProperties.cs#L356
            return new ExtractedModFile(path, Encoding.UTF8.GetBytes(sb.ToString()));
        }

        protected static IEnumerable<string> ReadList(BinaryReader reader) {
            for (string item = reader.ReadString(); item.Length > 0; item = reader.ReadString()) yield return item;
        }
    }
}