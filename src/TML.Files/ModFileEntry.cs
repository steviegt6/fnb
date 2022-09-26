using TML.Files.Abstractions;

namespace TML.Files
{
    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IModFileEntry"/> implementation.
    /// </summary>
    public class ModFileEntry : IModFileEntry
    {
        public virtual string Name { get; set; }

        public virtual int Offset { get; set; }

        public virtual int Length { get; set; }

        public virtual int CompressedLength { get; set; }

        public virtual byte[]? CachedBytes { get; set; }
        
        public ModFileEntry(string name, int offset, int length, int compressedLength, byte[]? cachedBytes) {
            // ReSharper disable VirtualMemberCallInConstructor
            Name = name;
            Offset = offset;
            Length = length;
            CompressedLength = compressedLength;
            CachedBytes = cachedBytes;
        }
    }
}