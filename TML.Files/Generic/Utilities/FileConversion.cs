using System;
using System.IO;
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
            fixed (byte* pData = data)
            {
                // Get the width and height using pointers to the image data
                int width = *(int*)pData;
                int height = *(int*)pData;
                byte* pPixels = pData + 12;

                // Create a new image with the width and height of the image, and the same color type
                SKImageInfo info = new(width, height, SKColorType.Rgba8888);
                using SKBitmap imageMap = new(info);

                // Copy the pixels from the image data onto the Skia Bitmap
                IntPtr intPtr = (IntPtr)pPixels;
                SKImageInfo oldInfo = new(width, height, SKColorType.Rgba8888);
                imageMap.InstallPixels(oldInfo, intPtr);

                // Encode and save the image
                using (SKData encodedImage = imageMap.Encode(SKEncodedImageFormat.Png, 100))
                using (Stream stream = File.OpenWrite(Path.ChangeExtension(properPath, ".png")))
                    encodedImage.SaveTo(stream);
            }
        }
    }
}