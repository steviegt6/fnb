using System;

namespace TML.Files.ModLoader.Files
{
    /// <summary>
    ///     Struct containing a mod's internal name, version, and the version of tModLoader it was built on.
    /// </summary>
    public struct ModData
    {
        /// <summary>
        ///     The tModLoader mod's name.
        /// </summary>
        public string modName;

        /// <summary>
        ///     The version of the tModLoader mod.
        /// </summary>
        public Version modVersion;

        /// <summary>
        ///     The version of tModLoader the mod was built on.
        /// </summary>
        public Version modLoaderVersion;

        /// <summary>
        /// </summary>
        /// <param name="modName"></param>
        /// <param name="modVersion"></param>
        /// <param name="modLoaderVersion"></param>
        public ModData(string modName, Version modVersion, Version modLoaderVersion)
        {
            this.modName = modName;
            this.modVersion = modVersion;
            this.modLoaderVersion = modLoaderVersion;
        }
    }
}