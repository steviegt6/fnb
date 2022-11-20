namespace TML.Files.Extraction.Extractors;

/// <summary>
///     Preserves a file's original data during extraction.
/// </summary>
public class RawByteFileExtractor : IFileExtractor
{
    /// <inheritdoc cref="IFileExtractor.ShouldExtract"/>
    public bool ShouldExtract(TModFileEntry entry) {
        return true;
    }

    /// <inheritdoc cref="IFileExtractor.Extract"/>
    public TModFileData Extract(TModFileEntry entry, byte[] data) {
        return new TModFileData(entry.Path, data);
    }
}