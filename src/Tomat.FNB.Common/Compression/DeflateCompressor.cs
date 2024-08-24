using System;
using System.Runtime.InteropServices;

namespace Tomat.FNB.Common.Compression;

public sealed class DeflateCompressor(int compressionLevel) : LibDeflateCompressor(compressionLevel)
{
    protected override nuint CompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output
    )
    {
        return libdeflate_deflate_compress(
            Compressor,
            MemoryMarshal.GetReference(input),
            (nuint)input.Length,
            ref MemoryMarshal.GetReference(output),
            (nuint)output.Length
        );
    }

    protected override nuint GetBoundCore(nuint inputLength)
    {
        return libdeflate_deflate_compress_bound(Compressor, inputLength);
    }
}