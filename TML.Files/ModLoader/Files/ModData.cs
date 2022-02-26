using System;

namespace TML.Files.ModLoader.Files
{
    /// <summary>
    ///     Class containing a mod's internal name, version, and the version of tModLoader it was built on.
    /// </summary>
    public readonly struct ModData
    {
        /// <summary>
        ///     The tModLoader mod's name.
        /// </summary>
        public readonly string ModName;

        /// <summary>
        ///     The version of the tModLoader mod.
        /// </summary>
        public readonly Version ModVersion;

        /// <summary>
        ///     The version of tModLoader the mod was built on.
        /// </summary>
        public readonly Version ModLoaderVersion;

        /// <summary>
        ///     Constructs a new <see cref="ModData"/> instance.
        /// </summary>
        public ModData(string modName, Version modVersion, Version modLoaderVersion)
        {
            ModName = modName;
            ModVersion = modVersion;
            ModLoaderVersion = modLoaderVersion;
        }
    }
}