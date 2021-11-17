using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace TML.Files.Generic.Utilities
{
    /// <summary>
    ///     Provides helper-methods for converting file formats. Usually pertains to tModLoader, though they are not limited to tModLoader.
    /// </summary>
    public static class FileConversion
    {
        /// <summary>
        ///     Converts a .raw file to a .png. The specific .raw format is the one used by tML, which contains raw data about the RGBA values of each individual pixel. <br />
        /// </summary>
        public static unsafe void ConvertRawToPng(byte[] data, string properPath)
        {
            ReadOnlySpan<byte> dataSpan = data;
            int width = MemoryMarshal.Read<int>(dataSpan[4..8]);
            int height = MemoryMarshal.Read<int>(dataSpan[8..12]);
            ReadOnlySpan<byte> oldPixels = dataSpan[12..];

            SKImageInfo info = new(width, height, SKColorType.Rgba8888);
            using SKBitmap imageMap = new(info);

            fixed (byte* ptr = oldPixels)
            {
                IntPtr intPtr = (IntPtr)ptr;
                SKImageInfo oldInfo = new(width, height, SKColorType.Rgba8888);
                imageMap.InstallPixels(oldInfo, intPtr);
            }

            using (SKData encodedImage = imageMap.Encode(SKEncodedImageFormat.Png, 100))
            using (Stream stream = File.OpenWrite(Path.ChangeExtension(properPath, ".png")))
                encodedImage.SaveTo(stream);
        }
    }
}