using System.IO;

namespace TML.Files.Extraction.Packers;

public abstract class BaseFilePacker : IFilePacker
{
    public abstract bool ShouldPack(TModFileData data);

    public TModFileData Pack(TModFileData data) {
        string resName = data.Path;
        using var ms = new MemoryStream();
        Pack(ref resName, data.Data, ms);
        return new TModFileData(resName, ms.ToArray());
    }

    protected abstract void Pack(ref string resName, byte[] from, MemoryStream to);
}