using System;
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
        public virtual string Header { get; set; } = ModFileWriter.MAGIC_HEADER;

        public virtual string ModLoaderVersion { get; set; } = "";

        public virtual byte[] Hash { get; set; } = Array.Empty<byte>();

        public virtual byte[] Signature { get; set; } = Array.Empty<byte>();

        public virtual string Name { get; set; } = "";

        public virtual string Version { get; set; } = "0.0.0.0";

        public virtual IEnumerable<IModFileEntry> Files { get; set; } = new List<IModFileEntry>();
    }
}