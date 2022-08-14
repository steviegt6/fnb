namespace TML.Files
{
    public class ModFileEntry
    {
        public readonly string Name;
        public int Offset;
        public readonly int Length;
        public readonly int CompressedLength;
        public byte[]? CachedBytes;

        public ModFileEntry(string name, int offset, int length, int compressedLength, byte[]? cachedBytes = null)
        {
            Name = name;
            Offset = offset;
            Length = length;
            CompressedLength = compressedLength;
            CachedBytes = cachedBytes;
        }

        public bool IsCompressed => Length != CompressedLength;
    }
}