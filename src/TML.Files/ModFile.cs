using System.Collections.Generic;
using TML.Files.Abstractions;

namespace TML.Files
{
    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IModFile"/> implementation.
    /// </summary>
    /// <remarks>
    ///     An instance of this class should not be instantiated directly. Please use a <see cref="ModFileReader"/>.
    /// </remarks>
    public class ModFile : IModFile
    {
        // All properties are set to null initially because this class may not be directly instantiated.
        // The only way to instantiate this class is through a ModFileReader (without the use of API-unsupported methods such as reflection, of course).

        public virtual string Header { get; internal set; } = null!;

        public virtual string ModLoaderVersion { get; internal set; } = null!;

        public virtual byte[] Hash { get; internal set; } = null!;

        public virtual byte[] Signature { get; internal set; } = null!;

        public virtual string Name { get; internal set; } = null!;

        public virtual string Version { get; internal set; } = null!;

        public virtual IEnumerable<IModFileEntry> Files { get; internal set; } = null!;

        internal ModFile() { }
    }
}