using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tomat.Files.Tmod;

public static class ContentConverters
{
    private const int rawimg_version = 1;

    public static bool Convert(ref string resourceName, FileStream src, MemoryStream dst)
    {
        switch (Path.GetExtension(resourceName).ToLower())
        {
            case ".png":
                if (resourceName != "icon.png")
                {
                    ToRaw(src, dst);
                    resourceName = Path.ChangeExtension(resourceName, "rawimg");
                    return true;
                }

                src.Position = 0;
                return false;

            default:
                return false;
        }
    }

    private static void ToRaw(Stream src, Stream dst)
    {
        using var image = Image.Load<Rgba32>(src);

        using BinaryWriter writer = new(dst);
        writer.Write(rawimg_version);
        writer.Write(image.Width);
        writer.Write(image.Height);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var color = image[x, y];

                if (color.A == 0)
                {
                    color = new Rgba32(0, 0, 0, 0);
                }

                dst.WriteByte(color.R);
                dst.WriteByte(color.G);
                dst.WriteByte(color.B);
                dst.WriteByte(color.A);
            }
        }
    }
}
