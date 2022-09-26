using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        // TODO: Make these configurable? tModLoader shouldn't care, but parity is important.
        public const uint MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1 kb
        public const float COMPRESSION_TRADEOFF = 0.9f;
        public const string HEADER = "TMOD";

        public virtual string Header { get; set; } = HEADER;

        public virtual string ModLoaderVersion { get; set; } = "";

        public virtual byte[] Hash { get; set; } = Array.Empty<byte>();

        public virtual byte[] Signature { get; set; } = Array.Empty<byte>();

        public virtual string Name { get; set; } = "";

        public virtual string Version { get; set; } = "0.0.0.0";

        public virtual IList<IModFileEntry> Files { get; set; } = new List<IModFileEntry>();

        public void AddFile(string fileName, byte[] data) {
            fileName = fileName.Trim().Replace('\\', '/'); // basic sanitization

            int size = data.Length;
            if (size > MINIMUM_COMPRESSION_SIZE && ShouldCompress(fileName, data)) {
                using MemoryStream mem = new(data.Length);
                using (DeflateStream def = new(mem, CompressionMode.Compress)) def.Write(data, 0, data.Length);

                byte[] compressed = mem.ToArray();
                if (compressed.Length < size * COMPRESSION_TRADEOFF) data = compressed;
            }

            Files.Add(new ModFileEntry(fileName, -1, size, data.Length, data));
        }

        public bool ShouldCompress(string fileName, byte[] data) {
            return !new[] {".png", ".mp3", ".ogg", ".rawimg"}.Contains(Path.GetExtension(fileName));
        }
    }
}