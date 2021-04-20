using System.IO;
using System.IO.Compression;

namespace TML.Files.Generic.Utilities
{
    public static class FileUtilities
    {
        public static byte[] DecompressFile(byte[] data, int decompressedSize)
        {
            MemoryStream dataStream = new(data);
            byte[] decompressed = new byte[decompressedSize];

            using DeflateStream deflatedStream = new(dataStream, CompressionMode.Decompress);
            deflatedStream.Read(decompressed, 0, decompressedSize);

            return decompressed;
        }
    }
}
