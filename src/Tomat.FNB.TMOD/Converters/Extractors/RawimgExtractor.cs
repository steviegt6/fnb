using System;
using System.IO;
using System.Runtime.InteropServices;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tomat.FNB.TMOD.Converters.Extractors;

public sealed unsafe class RawimgExtractor : IFileConverter
{
    private RawimgExtractor() { }

    bool IFileConverter.ShouldConvert(string path, byte[] data)
    {
        return Path.GetExtension(path) == ".rawimg";
    }

    (string path, byte[] data) IFileConverter.Convert(string path, byte[] data)
    {
        fixed (byte* pData = data)
        {
            var width  = *(int*)(pData + 4);
            var height = *(int*)(pData + 8);
            var pImage = pData + 12;

            using var image = Image.WrapMemory<Rgba32>(pImage, width * height * 4, width, height);

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return (Path.ChangeExtension(path, ".png"), ms.ToArray());
        }
    }

    public static IFileConverter GetRawimgExtractor()
    {
        if (OperatingSystem.IsWindows() && Environment.Is64BitProcess && RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return new FpngExtractor();
        }

        return new RawimgExtractor();
    }
}