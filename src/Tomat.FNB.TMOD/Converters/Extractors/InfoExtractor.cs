using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tomat.FNB.TMOD.Converters.Extractors;

public sealed class InfoExtractor : IFileConverter
{
    private delegate void Reader(BinaryReader reader, ref string key, out string? value);

    private static readonly Reader list_reader = (BinaryReader reader, ref string _, out string? value) =>
    {
        value = string.Join(", ", ReadList(reader));
        return;

        static IEnumerable<string> ReadList(BinaryReader r)
        {
            for (var item = r.ReadString(); item.Length > 0; item = r.ReadString())
            {
                yield return item;
            }
        }
    };

    private static readonly Reader true_reader = (BinaryReader _, ref string _, out string? value) =>
    {
        value = "true";
    };

    private static readonly Reader false_reader = (BinaryReader _, ref string key, out string? value) =>
    {
        key   = key[1..];
        value = "false";
    };

    private static readonly Reader mod_side_reader = (BinaryReader reader, ref string _, out string? value) =>
    {
        value = reader.ReadByte() switch
        {
            0 => "Both",
            1 => "Client",
            2 => "Server",
            3 => "NoSync",
            _ => null,
        };
    };

    private static readonly Reader description_reader = (BinaryReader reader, ref string _, out string? value) =>
    {
        reader.ReadString();
        value = null;
    };

    private static readonly Reader string_reader = (BinaryReader reader, ref string _, out string? value) =>
    {
        value = reader.ReadString();
    };

    private static readonly Dictionary<string, Reader> readers = new()
    {
        { "dllReferences", list_reader },
        { "modReferences", list_reader },
        { "weakReferences", list_reader },
        { "sortAfter", list_reader },
        { "sortBefore", list_reader },

        { "noCompile", true_reader },
        { "includeSource", true_reader },
        { "includePDB", true_reader },
        { "beta", true_reader },
        { "translationMod", true_reader },

        { "!hideCode", false_reader },
        { "!hideResources", false_reader },
        { "!playableOnPreview", false_reader },

        { "side", mod_side_reader },

        // Identical to description.txt, which is already included - pointless
        // to extract.
        { "description", description_reader },

        // {"eacPath", STRING_READER},
        { "buildVersion", string_reader },
        { "displayName", string_reader },
        { "author", string_reader },
        { "version", string_reader },
        { "homepage", string_reader },
        { "languageVersion", string_reader },
    };

    bool IFileConverter.ShouldConvert(string path, byte[] data)
    {
        return path == "Info";
    }

    public (string path, byte[] data) Convert(string path, byte[] data)
    {
        var sb = new StringBuilder();
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            {
                for (var key = reader.ReadString(); key.Length > 0; key = reader.ReadString())
                {
                    if (readers.TryGetValue(key, out var read))
                    {
                        read(reader, ref key, out var value);
                        sb.AppendLine($"{key} = {value}");
                    }
                    else
                    {
                        sb.AppendLine($"FNB ERROR: unknown Info key \"{key}\"");
                    }
                }
            }
        }
        return ("build.txt", Encoding.UTF8.GetBytes(sb.ToString()));
    }
}