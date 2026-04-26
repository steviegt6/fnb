using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tomat.Files.Tmod;

// TODO: Check for duplicate references!

public sealed partial class BuildProperties
{
    public List<string> DllReferences { get; set; } = [];

    public List<ModReference> ModReferences { get; set; } = [];

    public List<ModReference> WeakReferences { get; set; } = [];

    public string[] SortAfter { get; set; } = [];

    public string[] SortBefore { get; set; } = [];

    public string[] BuildIgnores { get; set; } = [];

    public string Author { get; set; } = string.Empty;

    public Version Version { get; set; } = new(1, 0);

    public string DisplayName { get; set; } = "";

    public bool HideCode { get; set; }

    public bool HideResources { get; set; }

    public bool IncludeSource { get; set; }

    public string EacPath { get; set; } = string.Empty;

    public string Homepage { get; set; } = "";

    public string Description { get; set; } = "";

    public ModSide Side { get; set; } = ModSide.Both;

    public bool PlayableOnPreview { get; set; } = true;

    public bool TranslationMod { get; set; }

    public string ModSource { get; set; } = string.Empty;

    public IEnumerable<ModReference> GetReferences(bool includeWeak)
    {
        return includeWeak ? ModReferences.Concat(WeakReferences) : ModReferences;
    }

    public IEnumerable<string> GetReferenceNames(bool includeWeak)
    {
        return GetReferences(includeWeak).Select(dep => dep.Mod);
    }

    public void AddDllReference(string name)
    {
        DllReferences.Add(name);
    }

    public void AddModReference(string modName, bool weak)
    {
        if (weak)
        {
            WeakReferences.Add(ModReference.Parse(modName));
        }
        else
        {
            ModReferences.Add(ModReference.Parse(modName));
        }
    }

    public bool IgnoreFile(string resource)
    {
        // TODO: Smarter handling of lib/ filtering.
        return BuildIgnores.Any(fileMask => FitsMask(resource, fileMask)) || DllReferences.Contains("lib/" + Path.GetFileName(resource));

        static bool FitsMask(string fileName, string fileMask)
        {
            var pattern =
                '^' +
                Regex.Escape(
                          fileMask.Replace(".", "__DOT__")
                                  .Replace("*", "__STAR__")
                                  .Replace("?", "__QM__")
                      )
                     .Replace("__DOT__", "[.]")
                     .Replace("__STAR__", ".*")
                     .Replace("__QM__", ".")
              + '$';
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(fileName);
        }
    }
}
