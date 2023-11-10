using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TML.Files.Extraction.Extractors;

/// <summary>
///     Extracts an <c>Info</c> file into a <c>build.txt</c> file.
/// </summary>
public class InfoFileExtractor : IFileExtractor
{
    #region KVP Reading

    /// <summary>
    ///     Handles reading a key-value pair from the <c>Info</c> file.
    /// </summary>
    public delegate void KvpReader(BinaryReader r, ref string k, out string? v);

    /// <summary>
    ///     Reads a list.
    /// </summary>
    public static readonly KvpReader LIST_READER = (BinaryReader r, ref string _, out string? v) =>
    {
        static IEnumerable<string> ReadList(BinaryReader r) {
            for (var item = r.ReadString(); item.Length > 0; item = r.ReadString()) yield return item;
        }

        v = string.Join(", ", ReadList(r));
    };

    /// <summary>
    ///     Reads a true boolean value.
    /// </summary>
    public static readonly KvpReader TRUE_READER = (BinaryReader _, ref string _, out string? v) => v = "true";

    /// <summary>
    ///     Reads a false boolean value.
    /// </summary>
    public static readonly KvpReader FALSE_READER = (BinaryReader _, ref string k, out string? v) =>
    {
        k = k.Substring(1);
        v = "false";
    };

    /// <summary>
    ///     Reads a ModSide value.
    /// </summary>
    public static readonly KvpReader MOD_SIDE_READER = (BinaryReader r, ref string _, out string? v) =>
    {
        v = r.ReadByte() switch
        {
            0 => "Both",
            1 => "Client",
            2 => "Server",
            3 => "NoSync",
            _ => null,
        };
    };

    /// <summary>
    ///     Skips reading a value.
    /// </summary>
    public static readonly KvpReader SKIP_READER = (BinaryReader _, ref string _, out string? v) => v = null;

    /// <summary>
    ///     Reads a string value.
    /// </summary>
    public static readonly KvpReader STRING_READER = (BinaryReader r, ref string _, out string? v) => v = r.ReadString();

    /// <summary>
    ///     Describes the expected keys in the <c>Info</c> file and how they should be read.
    /// </summary>
    public static readonly Dictionary<string, KvpReader> KVP_READERS = new()
    {
        {"dllReferences", LIST_READER},
        {"modReferences", LIST_READER},
        {"weakReferences", LIST_READER},
        {"sortAfter", LIST_READER},
        {"sortBefore", LIST_READER},

        {"noCompile", TRUE_READER},
        {"includeSource", TRUE_READER},
        {"includePDB", TRUE_READER},
        {"beta", TRUE_READER},

        {"!hideCode", FALSE_READER},
        {"!hideResources", FALSE_READER},
        {"!playableOnPreview", FALSE_READER},

        {"side", MOD_SIDE_READER},

        // Identical to description.txt, which is already included - pointless to extract.
        {"description", SKIP_READER},

        // TODO: eacPath and buildVersion aren't properties set by the user. Should we still include these? Probably not...
        // {"eacPath", STRING_READER},
        // {"buildVersion", STRING_READER},
        {"displayName", STRING_READER},
        {"author", STRING_READER},
        {"version", STRING_READER},
        {"homepage", STRING_READER},
        {"languageVersion", STRING_READER},
    };

    #endregion
    
    /// <inheritdoc cref="IFileExtractor.ShouldExtract"/>
    public bool ShouldExtract(TModFileEntry entry) {
        return entry.Path == "Info";
    }

    /// <inheritdoc cref="IFileExtractor.Extract"/>
    public TModFileData Extract(TModFileEntry entry, byte[] data) {
        StringBuilder sb = new();
        using MemoryStream ms = new(data);
        using BinaryReader r = new(ms);

        for (var key = r.ReadString(); key.Length > 0; key = r.ReadString()) {
            string? value;
            if (!KVP_READERS.TryGetValue(key, out var reader))
                value = null;
            else
                reader.Invoke(r, ref key, out value);
            if (value is not null) sb.AppendLine($"{key} = {value}");
        }

        return new TModFileData("build.txt", Encoding.UTF8.GetBytes(sb.ToString()));
    }
}