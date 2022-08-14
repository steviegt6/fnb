using System.IO;

namespace TML.Files.Abstractions.Extensions
{
    /// <summary>
    ///     Utility extensions for <see cref="IModFileReader"/>/
    /// </summary>
    public static class ModFileReaderExtensions
    {
        public static IModFile ReadFromPath(this IModFileReader reader, string path) {
            if (!File.Exists(path)) throw new FileNotFoundException("Cannot read .tmod file from path because the file does not exist: " + path);
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return reader.Read(stream);
        }
    }
}