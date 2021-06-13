using System.IO;
using System.IO.Compression;

namespace TML.Files.Generic.Utilities
{
    public static class FileUtilities
    {
        /// <summary>
        ///     Uses a <see cref="MemoryStream"/> and <see cref="DeflateStream"/> to decompress a file, given the data and decompressed size.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="decompressedSize"></param>
        /// <returns></returns>
        public static byte[] DecompressFile(byte[] data, int decompressedSize)
        {
            MemoryStream dataStream = new(data);
            byte[] decompressed = new byte[decompressedSize];

            using DeflateStream deflatedStream = new(dataStream, CompressionMode.Decompress);
            deflatedStream.Read(decompressed, 0, decompressedSize);

            return decompressed;
        }
        
        /// <summary>
        ///     Uses a <see cref="MemoryStream"/> and <see cref="DeflateStream"/> to compress a file, given the data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] CompressFile(byte[] data)
        {
            MemoryStream dataStream = new(data);
            MemoryStream compressStream = new();

            DeflateStream deflateStream = new(compressStream, CompressionMode.Compress);
            dataStream.CopyTo(deflateStream);
            deflateStream.Dispose();

            return compressStream.ToArray();
        }
    }
}