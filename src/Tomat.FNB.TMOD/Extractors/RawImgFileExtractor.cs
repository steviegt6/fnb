using System.IO;

using Tomat.FNB.Common.BinaryData;

namespace Tomat.FNB.TMOD.Extractors;

public sealed class RawImgFileExtractor : ITmodFileExtractor
{
    bool ITmodFileExtractor.CanExtract(TmodFileEntry entry)
    {
        return Path.GetExtension(entry.Path) == ".rawimg";
    }

    TmodFileData ITmodFileExtractor.Extract(TmodFileEntry entry, IDataView data)
    {
        throw new System.NotImplementedException();
    }
}
