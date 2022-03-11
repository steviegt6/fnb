using System;

namespace TML.Files
{
    /// <summary>
    ///     Class containing a mod's internal name, version, and the version of tModLoader it was built on.
    /// </summary>
    public class ModData
    {
        /// <summary>
        ///     The tModLoader mod's name.
        /// </summary>
        public string ModName;

        /// <summary>
        ///     The version of the tModLoader mod.
        /// </summary>
        public Version ModVersion;

        /// <summary>
        ///     The version of tModLoader the mod was built on.
        /// </summary>
        public Version ModLoaderVersion;

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