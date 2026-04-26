using System;
using System.Collections.Generic;

namespace Tomat.Files.Tmod.Manifest;

public readonly record struct ModReference(string ModName, Version? TargetVersion)
{
    public override string ToString()
    {
        return TargetVersion is null ? ModName : $"{ModName}@{TargetVersion}";
    }
}

public sealed class BuildProperties
{
    private static readonly Version default_version = new(1, 0);
    
    // Metadata
    public string DisplayName { get; set; } = "";

    public string Author { get; set; } = "";
    
    public Version Version { get; set; } = default_version;
    
    public string Homepage { get; set; } = "";
    
    public string Description { get; set; } = "";
    
    public ModSide Side { get; set; } = ModSide.Both;

    public bool PlayableOnPreview { get; set; } = true;
    
    public bool TranslationMod { get; set; }

    // References
    public List<string> DllReferences { get; } = [];
    
    public List<ModReference> ModReferences { get; } = [];
    
    public List<ModReference> WeakReferences { get; } = [];
    
    public List<string> SortAfter { get; } = [];
    
    public List<string> SortBefore { get; } = [];

    // Compilation
    public bool NoCompile { get; set; }
    
    public bool HideCode { get; set; }
    
    public bool HideResources { get; set; }
    
    public bool IncludeSource { get; set; }
    
    public List<string> BuildIgnores { get; } = [];
    
    // Compilation metadata
    public Version? BuildVersion { get; set; }

    public string EacPath { get; set; } = "";

    public string ModSource { get; set; } = "";

    // Unused?  I forget.
    public bool Beta { get; set; }

    public List<Diagnostic> Diagnostics { get; } = [];
}
