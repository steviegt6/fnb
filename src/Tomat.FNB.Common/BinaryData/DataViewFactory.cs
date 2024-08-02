using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

using LibDeflate;

namespace Tomat.FNB.Common.BinaryData;

public static class DataViewFactory
{
    private sealed class ByteArrayImpl : IDataView
    {
        public int Size => bytes.Length;

        private readonly byte[] bytes;
        private readonly bool   compressed;
        private readonly int    uncompressedLength;

        private ByteArrayImpl? compressedView;
        private ByteArrayImpl? decompressedView;

        public ByteArrayImpl(byte[] bytes, int uncompressedLength, bool compressed)
        {
            Debug.Assert(compressed || uncompressedLength == bytes.Length);

            this.bytes              = bytes;
            this.compressed         = compressed;
            this.uncompressedLength = uncompressedLength;
        }

        public IDataView CompressDeflate()
        {
            return compressed ? this : compressedView ??= Compress(bytes);
        }

        public IDataView DecompressDeflate()
        {
            return compressed ? decompressedView ??= Decompress(bytes, uncompressedLength) : this;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(bytes);
        }

        private static ByteArrayImpl Compress(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes.Length);
            using (var ds = new DeflateStream(ms, CompressionMode.Compress))
            {
                ds.Write(bytes, 0, bytes.Length);
            }

            // TODO: Can we use GetBuffer here?
            return new ByteArrayImpl(ms.ToArray(), bytes.Length, true);
        }

        private static ByteArrayImpl Decompress(byte[] bytes, int uncompressedLength)
        {
            var array = GC.AllocateUninitializedArray<byte>(uncompressedLength);

            using DeflateDecompressor ds = new();
            ds.Decompress(bytes, new Span<byte>(array), out _);
            return new ByteArrayImpl(array, uncompressedLength, false);
        }
    }

    public static class ByteArray
    {
        public static class Deflate
        {
            public static IDataView CreateCompressed(byte[] bytes, int uncompressedLength)
            {
                return new ByteArrayImpl(bytes, uncompressedLength, true);
            }
        }

        public static IDataView Create(byte[] bytes, int uncompressedLength)
        {
            return new ByteArrayImpl(bytes, uncompressedLength, false);
        }
    }
}