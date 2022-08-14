using System.Collections.Generic;

namespace TML.Files.Abstractions
{
    /// <summary>
    ///     Represents the bare data stored in a <c>.tmod</c> file. A <c>.tmod</c> file is functionally just an archive format.
    /// </summary>
    public interface IModFile
    {
        /// <summary>
        ///     The magic header at the start of the file, typically should be &quot;TMOD&quot;.
        /// </summary>
        string Header { get; }

        /// <summary>
        ///     The version of tModLoader that was used to pack this file. Compatible with <see cref="System.Version.Parse(string)"/>.
        /// </summary>
        string ModLoaderVersion { get; }

        /// <summary>
        ///     A computed SHA1 file hash, calculated with 280 empty bytes after the header and mod loader version.
        /// </summary>
        byte[] Hash { get; }

        /// <summary>
        ///     Mod Browser-signed signature.
        /// </summary>
        byte[] Signature { get; }

        /// <summary>
        ///     The mod's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The mod's version. Compatible with <see cref="System.Version.Parse(string)"/>
        /// </summary>
        string Version { get; }

        /// <summary>
        ///     A collection of each file stored within this <c>.tmod</c> file.
        /// </summary>
        IEnumerable<IModFileEntry> Files { get; }
    }
}