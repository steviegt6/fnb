namespace TML.Files.Extraction;

/// <summary>
///     Handles the extraction of a <see cref="TModFileEntry"/> into a <see cref="TModFileData"/>.
/// </summary>
public interface IFileExtractor
{
    /// <summary>
    ///     Whether this <see cref="IFileExtractor"/> should extract the given <paramref name="entry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="TModFileEntry"/> to determine extraction for.</param>
    /// <returns>Whether this <see cref="IFileExtractor"/> should handle extraction.</returns>
    bool ShouldExtract(TModFileEntry entry);
    
    /// <summary>
    ///     Extracts the given <paramref name="entry"/> into a <see cref="TModFileData"/>.
    /// </summary>
    /// <param name="entry">The <see cref="TModFileEntry"/> to extract.</param>
    /// <param name="data">The uncompressed data.</param>
    /// <returns>The extracted file data and relative path, as a <see cref="TModFileData"/> record.</returns>
    TModFileData Extract(TModFileEntry entry, byte[] data);
}