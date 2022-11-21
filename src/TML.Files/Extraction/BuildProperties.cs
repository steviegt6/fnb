using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TML.Files.Extraction;

public sealed class BuildProperties
{
    #region Properties

    public List<string> DllReferences { get; set; } = new();

    public List<ModReference> ModReferences { get; set; } = new();

    public List<ModReference> WeakReferences { get; set; } = new();

    public string[] SortAfter { get; set; } = Array.Empty<string>();

    public string[] SortBefore { get; set; } = Array.Empty<string>();

    public string[] BuildIgnores { get; set; } = Array.Empty<string>();

    public string Author { get; set; } = string.Empty;

    public Version Version { get; set; } = new(1, 0, 0, 0);

    public string DisplayName { get; set; } = string.Empty;

    public bool NoCompile { get; set; } = false;

    public bool HideCode { get; set; } = false;

    public bool HideResources { get; set; } = false;

    public bool IncludeSource { get; set; } = false;

    public string EacPath { get; set; } = string.Empty;

    public bool Beta { get; set; } = false;

    public string Homepage { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ModSide Side { get; set; } = ModSide.Both;

    #endregion

    public IEnumerable<ModReference> Refs(bool includeWeak) {
        return includeWeak ? ModReferences.Concat(WeakReferences) : ModReferences;
    }

    public IEnumerable<string> RefNames(bool includeWeak) {
        return Refs(includeWeak).Select(x => x.Mod);
    }

    public void AddDllReference(string name) {
        DllReferences.Add(name);
    }

    public void AddModReference(string modName, bool weak) {
        (weak ? WeakReferences : ModReferences).Add(ModReference.Parse(modName));
    }

    public byte[] ToBytes(string buildVersion) {
        using MemoryStream memStream = new();
        using BinaryWriter writer = new(memStream);

        if (DllReferences.Count > 0) {
            writer.Write("dllReferences");
            WriteList(DllReferences, writer);
        }

        if (ModReferences.Count > 0) {
            writer.Write("modReferences");
            WriteList(ModReferences, writer);
        }

        if (WeakReferences.Count > 0) {
            writer.Write("weakReferences");
            WriteList(WeakReferences, writer);
        }

        if (SortAfter.Length > 0) {
            writer.Write("sortAfter");
            WriteList(SortAfter, writer);
        }

        if (SortBefore.Length > 0) {
            writer.Write("sortBefore");
            WriteList(SortBefore, writer);
        }

        if (Author.Length > 0) {
            writer.Write("author");
            writer.Write(Author);
        }

        writer.Write("version");
        writer.Write(Version.ToString());

        if (DisplayName.Length > 0) {
            writer.Write("displayName");
            writer.Write(DisplayName);
        }

        if (Homepage.Length > 0) {
            writer.Write("homepage");
            writer.Write(Homepage);
        }

        if (Description.Length > 0) {
            writer.Write("description");
            writer.Write(Description);
        }

        if (NoCompile) {
            writer.Write("noCompile");
        }

        if (!HideCode) {
            writer.Write("!hideCode");
        }

        if (!HideResources) {
            writer.Write("!hideResources");
        }

        if (IncludeSource) {
            writer.Write("includeSource");
        }

        if (EacPath.Length > 0) {
            writer.Write("eacPath");
            writer.Write(EacPath);
        }

        if (Side != ModSide.Both) {
            writer.Write("side");
            writer.Write((byte) Side);
        }

        writer.Write("buildVersion");
        writer.Write(buildVersion);

        writer.Write("");
        return memStream.ToArray();
    }

    public bool IsFileIgnored(string res) {
        return BuildIgnores.Any(x => FitsMask(res, x)) || DllReferences.Contains("lib/" + Path.GetFileName(res));
    }

    public static bool FitsMask(string fileName, string fileMask) {
        string escape = Regex.Escape(
            fileMask.Replace(".", "__DOT__")
                    .Replace("*", "__STAR__")
                    .Replace("?", "__QM__")
        );
        string pattern =
            '^'
            + escape
             .Replace("__DOT__", "[.]")
             .Replace("__STAR__", ".*")
             .Replace("__QM__", ".")
            + '$';
        return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(fileMask);
    }

    public static void VerifyRefs(List<string> refs) {
        if (refs.Count != refs.Distinct().Count()) throw new DuplicateNameException("Weak and strong references contain at least one matching mod!");
    }

    // Add weak and strong references that aren't in sortBefore to sortAfter.
    public static string[] DistinctSortAfter(BuildProperties props) {
        return props.RefNames(true).Where(x => !props.SortBefore.Contains(x)).Concat(props.SortAfter).Distinct().ToArray();
    }

    #region Utilities

    private static IEnumerable<string> ReadList(string value) {
        return value.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0);
    }

    private static void WriteList<T>(IEnumerable<T> collection, BinaryWriter writer)
        where T : notnull {
        foreach (var item in collection) writer.Write(item.ToString());
        writer.Write("");
    }

    #endregion

    public static BuildProperties ReadBuildInfo(Stream buildFile) {
        var props = new BuildProperties();
        var reader = new StreamReader(buildFile);

        for (string? line = reader.ReadLine(); line is not null; line = reader.ReadLine()) {
            if (string.IsNullOrWhiteSpace(line)) continue;

            int split = line.IndexOf('=');
            string prop = line.Substring(0, split).Trim();
            string val = line.Substring(split + 1).Trim();
            if (val.Length == 0) continue;

            ProcessProperty(props, prop, val);
        }

        return props;
    }

    public static void ProcessProperty(BuildProperties props, string prop, string val) {
        if (string.IsNullOrEmpty(prop)) return;
        if (string.IsNullOrEmpty(val)) return;

        switch (prop) {
            case "dllReferences":
                props.DllReferences = ReadList(val).ToList();
                break;

            case "modReferences":
                props.ModReferences = ReadList(val).Select(ModReference.Parse).ToList();
                break;

            case "weakReferences":
                props.WeakReferences = ReadList(val).Select(ModReference.Parse).ToList();
                break;

            case "sortBefore":
                props.SortBefore = ReadList(val).ToArray();
                break;

            case "sortAfter":
                props.SortAfter = ReadList(val).ToArray();
                break;

            case "author":
                props.Author = val;
                break;

            case "version":
                props.Version = new Version(val);
                break;

            case "displayName":
                props.DisplayName = val;
                break;

            case "homepage":
                props.Homepage = val;
                break;

            case "noCompile":
                props.NoCompile = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "hideCode":
                props.HideCode = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "hideResources":
                props.HideResources = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "includeSource":
                props.IncludeSource = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "buildIgnore":
                props.BuildIgnores = val.Split(',')
                                        .Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar))
                                        .Where(s => s.Length > 0)
                                        .ToArray();
                break;

            case "side":
                if (!Enum.TryParse(val, true, out ModSide side)) throw new ArgumentException($"Side \"{val}\"does not match: Both, Client, Server, NoSync");
                props.Side = side;
                break;
        }
    }
}