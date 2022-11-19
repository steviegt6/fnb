namespace TML.Files.Extensions;

partial class Extensions
{
    public static bool IsCompressed(this TModFileEntry entry) {
        return entry.Length != entry.CompressedLength;
    }
}