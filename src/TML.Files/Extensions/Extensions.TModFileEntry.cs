namespace TML.Files.Extensions;

partial class Extensions
{
    /// <summary>
    ///     Determines whether the <paramref name="entry"/> is compressed by comparing the <see cref="TModFileEntry.Length"/> to the <see cref="TModFileEntry.CompressedLength"/>.
    /// </summary>
    /// <param name="entry">The <see cref="TModFileEntry"/> to check for compression.</param>
    /// <returns>Whether the <paramref name="entry"/> is compressed.</returns>
    public static bool IsCompressed(this TModFileEntry entry) {
        return entry.Length != entry.CompressedLength;
    }
}