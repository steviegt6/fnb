using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Tomat.Files.Tmod;

partial class BuildProperties
{
    public static BuildProperties ReadBuildInfo(string buildFile)
    {
        var properties = new BuildProperties();

        var lines = File.ReadAllLines(buildFile);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Divergence from tML: trim lines.
            line = line.Trim();

            // Divergence from tML: explicitly treat lines starting with '#' as
            //                      a comment.
            if (line.StartsWith("#"))
            {
                continue;
            }

            var split = line.IndexOf('=');
            if (split == -1)
            {
                continue;
            }

            var property = line[..split].Trim();
            var value = line[(split + 1)..].Trim();
            if (value.Length == 0)
            {
                continue;
            }

            ProcessProperty(properties, property, value, buildFile, i);
        }

        VerifyRefs(properties.GetReferenceNames(true).ToList());
        properties.SortAfter = GetDistinctRefs();

        return properties;

        // Adds (mod|weak)References that are not in sortBefore to sortAfter
        string[] GetDistinctRefs()
        {
            return properties.GetReferenceNames(true)
                             .Where(dep => !properties.SortBefore.Contains(dep))
                             .Concat(properties.SortAfter).Distinct().ToArray();
        }

        static void VerifyRefs(List<string> refs)
        {
            if (refs.Distinct().Count() != refs.Count)
            {
                throw new DuplicateNameException("Duplicate mod or weak references.");
            }
        }
    }

    private static void ProcessProperty(BuildProperties properties, string property, string value, string path, int line)
    {
        switch (property)
        {
            case "dllReferences":
                properties.DllReferences = ReadList(value).ToList();
                break;

            case "modReferences":
                var modRefs = ReadList(value);
                var modReferences = new List<ModReference>();
                foreach (var modRef in modRefs)
                {
                    try
                    {
                        modReferences.Add(ModReference.Parse(modRef));
                    }
                    catch (Exception e)
                    {
                    }
                }

                properties.ModReferences = modReferences;
                break;

            case "weakReferences":
                var weakRefs = ReadList(value);
                var weakReferences = new List<ModReference>();
                foreach (var weakRef in weakRefs)
                {
                    try
                    {
                        weakReferences.Add(ModReference.Parse(weakRef));
                    }
                    catch (Exception e)
                    {
                    }
                }

                properties.WeakReferences = weakReferences;
                break;

            case "sortBefore":
                properties.SortBefore = ReadList(value).ToArray();
                break;

            case "sortAfter":
                properties.SortAfter = ReadList(value).ToArray();
                break;

            case "author":
                properties.Author = value;
                break;

            case "version":
                try
                {
                    properties.Version = new Version(value);
                }
                catch (Exception e)
                {
                }

                break;

            case "displayName":
                properties.DisplayName = value;
                break;

            case "homepage":
                properties.Homepage = value;
                break;

            case "playableOnPreview":
                EnsureBooleanValue("playableOnPreview", value);
                properties.PlayableOnPreview = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "translationMod":
                EnsureBooleanValue("translationMod", value);
                properties.TranslationMod = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "hideCode":
                EnsureBooleanValue("hideCode", value);
                properties.HideCode = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "hideResources":
                EnsureBooleanValue("hideResources", value);
                properties.HideResources = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "includeSource":
                EnsureBooleanValue("includeSource", value);
                properties.IncludeSource = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;

            case "buildIgnore":
                properties.BuildIgnores = value.Split(',')
                                               .Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar))
                                               .Where(s => s.Length > 0)
                                               .ToArray();
                break;

            case "side":
                if (!Enum.TryParse<ModSide>(value, true, out var side))
                {
                }

                properties.Side = side;
                break;
        }

        return;

        void EnsureBooleanValue(string propertyName, string valueToCheck)
        {
            if (valueToCheck.ToLowerInvariant() is "true" or "false")
            {
                return;
            }
        }

        static IEnumerable<string> ReadList(string value)
        {
            return value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);
        }
    }

    public byte[] ToBytes(string buildVersion)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        if (DllReferences.Count > 0)
        {
            writer.Write("dllReferences");
            WriteList(DllReferences, writer);
        }

        if (ModReferences.Count > 0)
        {
            writer.Write("modReferences");
            WriteList(ModReferences, writer);
        }

        if (WeakReferences.Count > 0)
        {
            writer.Write("weakReferences");
            WriteList(WeakReferences, writer);
        }

        if (SortAfter.Length > 0)
        {
            writer.Write("sortAfter");
            WriteList(SortAfter, writer);
        }

        if (SortBefore.Length > 0)
        {
            writer.Write("sortBefore");
            WriteList(SortBefore, writer);
        }

        if (Author.Length > 0)
        {
            writer.Write("author");
            writer.Write(Author);
        }

        writer.Write("version");
        writer.Write(Version.ToString());
        if (DisplayName.Length > 0)
        {
            writer.Write("displayName");
            writer.Write(DisplayName);
        }

        if (Homepage.Length > 0)
        {
            writer.Write("homepage");
            writer.Write(Homepage);
        }

        if (Description.Length > 0)
        {
            writer.Write("description");
            writer.Write(Description);
        }

        if (!PlayableOnPreview)
        {
            writer.Write("!playableOnPreview");
        }

        if (TranslationMod)
        {
            writer.Write("translationMod");
        }

        if (!HideCode)
        {
            writer.Write("!hideCode");
        }

        if (!HideResources)
        {
            writer.Write("!hideResources");
        }

        if (IncludeSource)
        {
            writer.Write("includeSource");
        }

        if (EacPath.Length > 0)
        {
            writer.Write("eacPath");
            writer.Write(EacPath);
        }

        if (Side != ModSide.Both)
        {
            writer.Write("side");
            writer.Write((byte)Side);
        }

        if (ModSource.Length > 0)
        {
            writer.Write("modSource");
            writer.Write(ModSource);
        }

        writer.Write("buildVersion");
        writer.Write(buildVersion);

        writer.Write("");

        return memoryStream.ToArray();

        static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer)
        {
            foreach (var item in list)
            {
                writer.Write(item!.ToString()!);
            }

            writer.Write("");
        }
    }
}
