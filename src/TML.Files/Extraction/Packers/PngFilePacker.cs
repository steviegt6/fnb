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

    protected override void Pack(ref string resName, byte[] from, MemoryStream to) {
        resName = Path.ChangeExtension(resName, ".rawimg");

        using Image<Rgba32> image = Image.Load<Rgba32>(from);
        using BinaryWriter writer = new(to);

        writer.Write(RAWIMG_FORMAT_VERSION);
        writer.Write(image.Width);
        writer.Write(image.Height);

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