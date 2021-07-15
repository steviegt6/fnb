using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

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

            using Bitmap imageMap = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData bitmapData = imageMap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, imageMap.PixelFormat);

            for (int y = 0; y < bitmapData.Height; y++)
            {
                int currentLine = y * bitmapData.Stride;
                byte* row = (byte*) bitmapData.Scan0 + currentLine;
                for (int x = 0; x < bitmapData.Width; x++)
                {
                    int posRaw = x * 4;
                    int posNormal = posRaw + currentLine;

                    row[posRaw + 2] = oldPixels[posNormal + 0]; // R
                    row[posRaw + 1] = oldPixels[posNormal + 1]; // G
                    row[posRaw + 0] = oldPixels[posNormal + 2]; // B
                    row[posRaw + 3] = oldPixels[posNormal + 3]; // A
                }
            }

            imageMap.UnlockBits(bitmapData);

            imageMap.Save(Path.ChangeExtension(properPath, ".png"));
        }
    }
}