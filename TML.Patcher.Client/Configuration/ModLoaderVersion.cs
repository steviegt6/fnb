using System;
using System.Collections.Generic;
using System.IO;

namespace TML.Patcher.Client.Configuration
{
    /// <summary>
    ///     Represents a tModLoader version.
    /// </summary>
    /// <param name="PathResolver">Gets path to append.</param>
    /// <param name="Workshop">Whether this version uses the Steam Workshop.</param>
    /// <param name="VersionAliases">All aliases. Index zero is the standardized name and index one is the display name.</param>
    public readonly record struct ModLoaderVersion(Func<string, string> PathResolver, bool Workshop, params string[] VersionAliases)
    {
        public static Func<string, string> StandardResolver(string path) => qualified => Path.Combine(qualified, path);

        /// <summary>
        ///     tModLoader version 1.3. Standardized to "legacy".
        /// </summary>
        public static readonly ModLoaderVersion Legacy = new(
            path =>
                StandardResolver(Directory.Exists(Path.Combine(path, "ModLoader"))
                    ? "ModLoader"
                    : "tModLoader-1.3").Invoke(path),
            false,
            "legacy", "1.3", "tModLoader-1.3"
        );

        /// <summary>
        ///     tModLoader version 1.4. Standardized to "modern".
        /// </summary>
        public static readonly ModLoaderVersion Modern = new(
            StandardResolver("tModLoader"),
            true,
            "modern", "1.4", "beta", "stable", "tModLoader"
        );

        /// <summary>
        ///     tModLoader version 1.4, preview branch. Standardized to "preview".
        /// </summary>
        public static readonly ModLoaderVersion Preview = new(
            StandardResolver("tModLoader-preview"),
            true,
            "preview", "1.4-preview", "prev", "tModLoader-preview", "tModLoader-prev"
        );

        /// <summary>
        ///     tModLoader version 1.4, development environment. Standardized to "dev".
        /// </summary>
        public static readonly ModLoaderVersion Developer = new(
            StandardResolver("tModLoader-dev"),
            true,
            "dev", "1.4-dev", "development", "tModLoader-dev", "tModLoader-development"
        );

        public static readonly IEnumerable<ModLoaderVersion> Versions = new List<ModLoaderVersion>
        {
            Legacy,
            Modern,
            Preview,
            Developer
        }.AsReadOnly();
    }
}