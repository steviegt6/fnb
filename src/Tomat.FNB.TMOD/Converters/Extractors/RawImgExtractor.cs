using System;
using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tomat.FNB.TMOD.Converters.Extractors;

public static class RawImgExtractor
{
    private sealed class SixLaborsConverter : IFileConverter
    {
        public bool ShouldConvert(string path, Span<byte> data)
        {
            return Path.GetExtension(path) == extension;
        }

        public unsafe bool Convert(string path, Span<byte> data, Action<string, Span<byte>> onCovert)
        {
            fixed (byte* pData = data)
            {
                var width  = *(int*)(pData + 4);
                var height = *(int*)(pData + 4);
                var pImage = pData + 12;

                using var image = Image.WrapMemory<Rgba32>(pImage, width * height * 4, width, height);

                // We annoyingly must allocate an array here.
                // TODO: Can we estimate the PNG size to reduce buffer resizing?
                // TODO: We could use an alternative code path that just
                //       allocates a large span and use a custom stream.
                using var ms = new MemoryStream();
                image.SaveAsPng(ms);

                onCovert(Path.ChangeExtension(path, ".png"), ms.ToArray());
                return true;
            }
        }

        public unsafe (string path, byte[] data)? Convert(string path, Span<byte> data)
        {
            fixed (byte* pData = data)
            {
                var width  = *(int*)(pData + 4);
                var height = *(int*)(pData + 4);
                var pImage = pData + 12;

                using var image = Image.WrapMemory<Rgba32>(pImage, width * height * 4, width, height);

                using var ms = new MemoryStream();
                image.SaveAsPng(ms);

                return (Path.ChangeExtension(path, ".png"), ms.ToArray());
            }
        }
    }

    private const string extension = ".rawimg";

    public static IFileConverter GetInstance()
    {
        // TODO: re-add fpng
        return new SixLaborsConverter();
    }
}