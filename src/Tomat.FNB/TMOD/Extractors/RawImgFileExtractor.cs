using System;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tomat.FNB.TMOD.Extractors;

public sealed class RawImgFileExtractor : FileExtractor {
    public override bool ShouldExtract(TmodFileEntry entry) {
        return Path.GetExtension(entry.Path) == ".rawimg";
    }

    public override TmodFileData Extract(TmodFileEntry entry, byte[] data) {
        ReadOnlySpan<byte> span = data;
        var width = MemoryMarshal.Read<int>(span.Slice(4, 4));
        var height = MemoryMarshal.Read<int>(span.Slice(8, 4));
        var rgbaValues = data.AsMemory(12);

        using var image = Image.WrapMemory<Rgba32>(Configuration.Default, rgbaValues, width, height);

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return new TmodFileData(Path.ChangeExtension(entry.Path, ".png"), ms.ToArray());
    }
}
