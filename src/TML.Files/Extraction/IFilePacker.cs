using System.IO;

namespace TML.Files.Extraction;

public interface IFilePacker
{
    bool ShouldPack(TModFileData data);
    
    TModFileData Pack(TModFileData data);
}