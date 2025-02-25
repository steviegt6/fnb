using System;
using System.IO;

using Tomat.FNB.Deflate;

using static Tomat.FNB.Deflate.libdeflate;

namespace Tomat.FNB.Common.IO.Compression;

/// <summary>
///     DEFLATE decompressor using <c>libdeflate</c>.
/// </summary>
public sealed class LibDeflateDecompressor : IDecompressor
{
    private readonly nint pDecompressor = libdeflate_alloc_decompressor();

    public bool Decompress(Span<byte> compressedBytes, Span<byte> uncompressedBytes)
    {
        var result = libdeflate_deflate_decompress(
            pDecompressor,
            in compressedBytes.GetPinnableReference(),
            (nuint)compressedBytes.Length,
            ref uncompressedBytes.GetPinnableReference(),
            (nuint)uncompressedBytes.Length,
            out _
        );

        // if (read != (nuint)uncompressedBytes.Length)
        // {
        //     throw new IOException($"Failed to decompress bytes: read={read}, expected={uncompressedBytes.Length}");
        // }

        return result == LibDeflateResult.Success;
    }

    public void Dispose()
    {
        libdeflate_free_decompressor(pDecompressor);
    }
}