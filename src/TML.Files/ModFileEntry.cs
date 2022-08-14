using TML.Files.Abstractions;

namespace TML.Files
{
    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IModFileEntry"/> implementation.
    /// </summary>
    public class ModFileEntry : IModFileEntry
    {
        public string Name { get; init; } = "";

        public int Offset { get; internal set; } = 0;

        public int Length { get; init; } = 0;

        public int CompressedLength { get; init; } = 0;

        public byte[]? CachedBytes { get; internal set; } = null;

        public ModFileEntry() { }
    }
}