using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;
using TML.Files;
using TML.Files.Extraction;

namespace TML.Patcher.Extractors
{
    public class RawImgFileExtractor : IFileExtractor
    {
        public bool ShouldExtract(TModFileEntry entry) {
            return Path.GetExtension(entry.Path) == ".rawimg";
        }

        public unsafe TModFileData Extract(TModFileEntry entry, byte[] data) {
            ReadOnlySpan<byte> dataSpan = data;
            int width = MemoryMarshal.Read<int>(dataSpan.Slice(4, 8));
            int height = MemoryMarshal.Read<int>(dataSpan.Slice(8, 12));
            ReadOnlySpan<byte> oldPixels = dataSpan.Slice(12);

            SKImageInfo info = new(width, height, SKColorType.Rgba8888);
            using SKBitmap imageMap = new(info);

            fixed (byte* ptr = oldPixels) {
                SKImageInfo oldInfo = new(width, height, SKColorType.Rgba8888);
                imageMap.InstallPixels(oldInfo, (IntPtr) ptr);
            }

            using SKData encodedImage = imageMap.Encode(SKEncodedImageFormat.Png, 100);
            using MemoryStream stream = new();
            encodedImage.SaveTo(stream);
            return new TModFileData(Path.ChangeExtension(entry.Path, ".png"), stream.ToArray());
        }
    }
}