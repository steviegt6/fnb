using TML.Files.Abstractions;

namespace TML.Files
{
    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IModFileEntry"/> implementation.
    /// </summary>
    public class ModFileEntry : IModFileEntry
    {
        public string Name { get; set; } = "";

        public int Offset { get; set; }

        public int Length { get; set; }

        public int CompressedLength { get; set; }

        public byte[]? CachedBytes { get; set; }
    }
}