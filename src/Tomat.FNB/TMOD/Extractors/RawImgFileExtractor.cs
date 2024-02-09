using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Tomat.FNB.Util;

namespace Tomat.FNB.TMOD.Extractors;

public sealed class RawImgFileExtractor : FileExtractor {
    public override bool ShouldExtract(TmodFileEntry entry) {
        return Path.GetExtension(entry.Path) == ".rawimg";
    }

    public override unsafe TmodFileData Extract(TmodFileEntry entry, AmbiguousData<byte> data) {
        var pData = data.Pointer;
        var width = *(int*)(pData + 4);
        var height = *(int*)(pData + 8);

        using var image = Image.WrapMemory<Rgba32>(pData + 12, width * height * 4, width, height);

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return new TmodFileData(Path.ChangeExtension(entry.Path, ".png"), new AmbiguousData<byte>(ms.GetBuffer()));
    }
}
