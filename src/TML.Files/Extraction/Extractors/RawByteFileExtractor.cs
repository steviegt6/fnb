namespace TML.Files.Extraction.Extractors;

public class RawByteFileExtractor : IFileExtractor
{
    public bool ShouldExtract(TModFileEntry entry) {
        return true;
    }

    public TModFileData Extract(TModFileEntry entry, byte[] data) {
        return new TModFileData(entry.Path, data);
    }
}