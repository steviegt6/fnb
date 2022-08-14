using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TML.Files.Abstractions;

namespace TML.Files.Extractors
{
    public class InfoFileExtractor : IFileExtractor
    {
        protected readonly record struct InfoKey(string Key, InfoKey.ReadInfoValue Reader)
        {
            public delegate void ReadInfoValue(BinaryReader reader, ref string tag, out string? value);

            public static InfoKey List(string key) {
                return new InfoKey(
                    key,
                    (BinaryReader reader, ref string _, out string? value) => { value = string.Join(", ", ReadList(reader)); }
                );
            }

            public static InfoKey True(string key) {
                return new InfoKey(
                    key,
                    (BinaryReader reader, ref string _, out string? value) => { value = "true"; }
                );
            }

            public static InfoKey False(string key) {
                return new InfoKey(
                    key,
                    (BinaryReader reader, ref string tag, out string? value) =>
                    {
                        tag = tag[1..];
                        value = "false";
                    }
                );
            }

            public static InfoKey Side(string key) {
                return new InfoKey(
                    key,
                    (BinaryReader reader, ref string tag, out string? value) =>
                    {
                        value = reader.ReadByte() switch
                        {
                            0 => "Both",
                            1 => "Client",
                            2 => "Server",
                            3 => "NoSync",
                            _ => null,
                        };
                    }
                );
            }

            public static InfoKey Skip(string key) {
                return new InfoKey(key, (BinaryReader reader, ref string _, out string? value) => { value = null; });
            }

            public static InfoKey String(string key) {
                return new InfoKey(
                    key,
                    (BinaryReader reader, ref string tag, out string? value) => { value = reader.ReadString(); }
                );
            }
        }

        protected static InfoKey[] Keys =
        {
            InfoKey.List("dllReferences"),
            InfoKey.List("modReferences"),
            InfoKey.List("weakReferences"),
            InfoKey.List("sortAfter"),
            InfoKey.List("sortBefore"),

            InfoKey.True("noCompile"),
            InfoKey.True("includeSource"),
            InfoKey.True("includePDB"),
            InfoKey.True("beta"),

            InfoKey.False("!hideCode"),
            InfoKey.False("!hideResources"),
            InfoKey.False("!playableOnPreview"),

            InfoKey.Side("side"),

            // This is identical to description.txt, no point in actually reading it.
            InfoKey.Skip("description"),

            // TODO: eacPath and buildVersion aren't properties set by the user. Should we still include these? Probably not...
            // InfoKey.String("eacPath"),
            // InfoKey.String("buildVersion"),
            InfoKey.String("displayName"),
            InfoKey.String("author"),
            InfoKey.String("version"),
            InfoKey.String("homepage"),
            InfoKey.String("languageVersion"),
        };

        public bool ShouldExtract(IModFileEntry fileEntry) {
            // build.txt is turned into the "Info" file in the root directory.
            return fileEntry.Name == "Info";
        }

        public IExtractedModFile Extract(IModFileEntry fileEntry, byte[] data) {
            StringBuilder sb = new();
            using MemoryStream stream = new(data);
            using BinaryReader reader = new(stream);

            for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString()) {
                InfoKey key = Keys.FirstOrDefault(x => x.Key == tag);
                string? value;
                
                // TODO: Throw an exception or raise some sort of message if no reader for the tag is found?
                if (key == default) value = null;
                else key.Reader(reader, ref tag, out value);

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