using System;

namespace TML.Files.Specific.Data
{
    public struct BuildProperties
    {
        public string[] dllReferences;
        public string[] modReferences;
        public string[] weakReferences;
        public string[] sortAfter;
        public string[] sortBefore;
        public string[] buildIgnores;
        public string author;
        public Version version;
        public string displayName;
        public string homepage;
        public string description;
        public bool noCompile;
        public bool hideCode;
        public bool hideResources;
        public bool includeSource;
        public bool includePDB;
        public ModSide side;

        // hidden
        public string eacPath;
        public bool beta;
        public Version buildVersion;

        public BuildProperties(bool beta)
        {
            this.beta = beta;
            side = ModSide.Client;
            dllReferences = Array.Empty<string>();
            modReferences = Array.Empty<string>();
            weakReferences = Array.Empty<string>();
            sortAfter = Array.Empty<string>();
            sortBefore = Array.Empty<string>();
            buildIgnores = Array.Empty<string>();
            author = "noauthor";
            version = new Version(0, 0, 0, 1);
            displayName = "nodisplayname";
            homepage = "nohomepage";
            description = "nodesc";
            noCompile = false;
            hideCode = false;
            hideResources = false;
            includeSource = false;
            includePDB = false;
            eacPath = "noeacpath";
            buildVersion = new Version(0, 0, 0, 1);
        }
    }
}