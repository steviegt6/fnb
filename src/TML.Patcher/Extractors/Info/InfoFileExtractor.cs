using System.IO;
using System.Linq;
using System.Text;
using TML.Files;
using TML.Files.Abstractions;

namespace TML.Patcher.Extractors.Info
{
    public class InfoFileExtractor : IFileExtractor
    {
        
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
    }
}