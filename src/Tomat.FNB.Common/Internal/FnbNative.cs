using System;
using System.Runtime.InteropServices;

namespace Tomat.FNB.Common.Internal;

// ReSharper disable InconsistentNaming - This is a native interop file.
internal static unsafe partial class FnbNative
{
    private static partial class fnb_native
    {
        private const string lib_name = "fnb_native";

        /// <summary>
        ///     Encodes a raw array of RGBA32 pixels into a PNG image.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="data">
        ///     A pointer to a contiguous section of memory containing the raw image
        ///     data.
        /// </param>
        /// <param name="length">The resulting length of the PNG in memory.</param>
        /// <returns>A pointer to the PNG in memory.</returns>
        [LibraryImport(lib_name, EntryPoint = nameof(encode_png))]
        public static partial nint encode_png(nint width, nint height, byte* data, out nuint length);

        /// <summary>
        ///     Frees the memory allocated for a PNG image from
        ///     <see cref="encode_png"/>.
        /// </summary>
        /// <param name="data">A pointer to the PNG image in memory.</param>
        [LibraryImport(lib_name, EntryPoint = nameof(free_encoded_png))]
        public static partial void free_encoded_png(nint data);

        /// <summary>
        ///     Decompresses a DEFLATE-compressed buffer.
        /// </summary>
        /// <param name="in_data">
        ///     A pointer to the compressed data.
        /// </param>
        /// <param name="in_length">
        ///     The length of the compressed data.
        /// </param>
        /// <param name="out_data">
        ///     A pointer to the buffer to write the decompressed data to.
        /// </param>
        /// <param name="out_length">
        ///     The length of the buffer to write the decompressed data to.
        /// </param>
        /// <returns>
        ///     The length of the decompressed data.
        /// </returns>
        [LibraryImport(lib_name, EntryPoint = nameof(decompress_deflate))]
        public static unsafe partial nint decompress_deflate(byte* in_data, nint in_length, byte* out_data, nint out_length);
    }

    /// <summary>
    ///     Encodes a raw array of RGBA32 pixels into a PNG image.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="image">The image bytes.</param>
    /// <param name="pngBytes">The PNG.</param>
    public static void EncodePng(int width, int height, Span<byte> image, out byte[] pngBytes)
    {
        fixed (byte* pImage = image)
        {
            var pPng = fnb_native.encode_png(width, height, pImage, out var length);
            pngBytes = new byte[length];
            Marshal.Copy(pPng, pngBytes, 0, (int)length);
            fnb_native.free_encoded_png(pPng);
        }
    }

    /// <summary>
    ///     Decompresses a DEFLATE-compressed buffer.
    /// </summary>
    /// <param name="compressedData">The compressed data.</param>
    /// <param name="decompressedLength">
    ///     The length of the decompressed data.
    /// </param>
    /// <returns>The decompressed data.</returns>
    public static byte[] DecompressDeflate(Span<byte> compressedData, int decompressedLength)
    {
        var decompressedData = new byte[decompressedLength];
        fixed (byte* pCompressedData = compressedData)
        fixed (byte* pDecompressedData = decompressedData)
        {
            var length = fnb_native.decompress_deflate(pCompressedData, compressedData.Length, pDecompressedData, decompressedLength);
            if (length < 0)
            {
                throw new InvalidOperationException("Failed to decompress DEFLATE data.");
            }
            return decompressedData;
        }
    }
}