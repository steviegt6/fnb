namespace TML.Files.Extraction;

public interface IFileExtractor
{
    bool ShouldExtract(TModFileEntry entry);
    
    TModFileData Extract(TModFileEntry entry, byte[] data);
}