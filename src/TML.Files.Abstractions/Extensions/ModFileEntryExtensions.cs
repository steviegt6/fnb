namespace TML.Files.Abstractions.Extensions
{
    /// <summary>
    ///     Utility extensions for <see cref="IModFileEntry"/>.
    /// </summary>
    public static class ModFileEntryExtensions
    {
        public static bool Compressed(this IModFileEntry modFileEntry) {
            return modFileEntry.Length != modFileEntry.CompressedLength;
        }
    }
}