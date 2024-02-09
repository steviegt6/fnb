using System.Collections.Generic;
using System.IO;
using System.Text;
using Tomat.FNB.Util;

namespace Tomat.FNB.TMOD.Extractors;

public static class InfoFileExtractor {
    public delegate void Reader(BinaryReader reader, ref string key, out string? value);

    private static readonly Reader list_reader = (BinaryReader reader, ref string _, out string? value) => {
        value = string.Join(", ", readList(reader));
        return;

        static IEnumerable<string> readList(BinaryReader r) {
            for (var item = r.ReadString(); item.Length > 0; item = r.ReadString())
                yield return item;
        }
    };

    private static readonly Reader true_reader = (BinaryReader _, ref string _, out string? value) => {
        value = "true";
    };

    private static readonly Reader false_reader = (BinaryReader _, ref string key, out string? value) => {
        key = key[1..];
        value = "false";
    };

    private static readonly Reader mod_side_reader = (BinaryReader reader, ref string _, out string? value) => {
        value = reader.ReadByte() switch {
            0 => "Both",
            1 => "Client",
            2 => "Server",
            3 => "NoSync",
            _ => null,
        };
    };

    private static readonly Reader skip_reader = (BinaryReader _, ref string _, out string? value) => {
        value = null;
    };

    private static readonly Reader string_reader = (BinaryReader reader, ref string _, out string? value) => {
        value = reader.ReadString();
    };

    public static readonly Dictionary<string, Reader> READERS = new() {
        { "dllReferences", list_reader },
        { "modReferences", list_reader },
        { "weakReferences", list_reader },
        { "sortAfter", list_reader },
        { "sortBefore", list_reader },

        { "noCompile", true_reader },
        { "includeSource", true_reader },
        { "includePDB", true_reader },
        { "beta", true_reader },

        { "!hideCode", false_reader },
        { "!hideResources", false_reader },
        { "!playableOnPreview", false_reader },

        { "side", mod_side_reader },

        // Identical to description.txt, which is already included - pointless
        // to extract.
        { "description", skip_reader },

        // {"eacPath", STRING_READER},
        // {"buildVersion", STRING_READER},
        { "displayName", string_reader },
        { "author", string_reader },
        { "version", string_reader },
        { "homepage", string_reader },
        { "languageVersion", string_reader },
    };

    public static bool ShouldExtract(TmodFileEntry entry) {
        return entry.Path == "Info";
    }

    public static TmodFileData Extract(TmodFileEntry entry, AmbiguousData<byte> data) {
        var sb = new StringBuilder();

        using var reader = new BinaryReader(new MemoryStream(data.Array));

        for (var key = reader.ReadString(); key.Length > 0; key = reader.ReadString()) {
            string? value;
            if (!READERS.TryGetValue(key, out var readerFunc))
                value = null;
            else
                readerFunc(reader, ref key, out value);

            if (value is not null)
                sb.AppendLine($"{key} = {value}");
        }

        return new TmodFileData("build.txt", new AmbiguousData<byte>(Encoding.UTF8.GetBytes(sb.ToString())));
    }
}
