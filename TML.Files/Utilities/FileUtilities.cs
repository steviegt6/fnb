using System.IO;
using System.IO.Compression;
using System.Text;

namespace TML.Files.Utilities
{
    /// <summary>
    ///     Provides numerous helper-methods for messing with files.
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        ///     Uses a <see cref="MemoryStream"/> and <see cref="DeflateStream"/> to decompress a file, given the data and decompressed size.
        /// </summary>
        public static byte[] DecompressFile(byte[] data)
        {
            using MemoryStream decompressedStream = new();
            using MemoryStream compressStream = new(data);
            using DeflateStream deflateStream = new(compressStream, CompressionMode.Decompress);
            
            deflateStream.CopyTo(decompressedStream);
            
            return decompressedStream.ToArray();
        }

        /// <summary>
        ///     Uses a <see cref="MemoryStream"/> and <see cref="DeflateStream"/> to compress a file, given the data.
        /// </summary>
        public static byte[] CompressFile(byte[] data)
        {
            MemoryStream dataStream = new(data);
            MemoryStream compressStream = new();

            DeflateStream deflateStream = new(compressStream, CompressionMode.Compress);
            dataStream.CopyTo(deflateStream);
            deflateStream.Dispose();

            return compressStream.ToArray();
        }

        public static string ConvertToString(this byte[] array, Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;

            return encoding.GetString(array);
        }
    }
}