using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TML.Files.Extraction.Packers;

public class PngFilePacker : BaseFilePacker
{
    public const int RAWIMG_FORMAT_VERSION = 1;

    public override bool ShouldPack(TModFileData data) {
        return data.Path != "icon.png" && Path.GetExtension(data.Path) == ".png";
    }

    private  static Configuration configuration;
    static PngFilePacker()
    {
        configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;
    }

    protected override unsafe void Pack(ref string resName, byte[] from, MemoryStream to) {
        resName = Path.ChangeExtension(resName, ".rawimg");

        using Image<Rgba32> image = Image.Load<Rgba32>(configuration, from);
        using BinaryWriter writer = new(to);

        writer.Write(RAWIMG_FORMAT_VERSION);
        writer.Write(image.Width);
        writer.Write(image.Height);
        int totalPixels = image.Width * image.Height;

        if (to.Length < from.Length) {
            to.SetLength(from.Length);
        }

        if (to.TryGetBuffer(out ArraySegment<byte> buffer) && buffer.Count >= totalPixels && image.DangerousTryGetSinglePixelMemory(out var memory)) {
            fixed (byte* _ptr = buffer.Array!)
            fixed (Rgba32* _colorPtr = memory.Span)
            {
                int* dst = (int*)(_ptr + buffer.Offset);
                int* src = (int*)_colorPtr;
                for (nint i = 0, c = totalPixels / 4; i < c; i++) {
                    int col = src[i];
                    dst[i] = col == 0 ? 0 : col;
                }
            }
            GC.KeepAlive(image);
            return;
        }

        for (int y = 0; y < image.Height; y++)
        for (int x = 0; x < image.Width; x++) {
            var color = image[x, y];

            if (color.A == 0) color = new Rgba32(0, 0, 0, 0);
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }
    }
}