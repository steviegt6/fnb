using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tomat.FNB.TMOD.Extractors;

public sealed unsafe class RawImgFileExtractor : FileExtractor {
    public override bool ShouldExtract(TmodFileEntry entry) {
        return Path.GetExtension(entry.Path) == ".rawimg";
    }

    public override TmodFileData Extract(TmodFileEntry entry, byte[] data) {
        fixed (byte* ptr = data) {
            var width = Unsafe.ReadUnaligned<int>(ptr + 4);
            var height = Unsafe.ReadUnaligned<int>(ptr + 8);
            var rgbaValues = ptr + 12;

            using var image = Image.WrapMemory<Rgba32>(rgbaValues, data.Length - 12, width, height);

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return new TmodFileData(Path.ChangeExtension(entry.Path, ".png"), ms.ToArray());
        }
    }
}
