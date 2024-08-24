using System;
using System.Runtime.InteropServices;

namespace Tomat.FNB.Common.Compression;

public sealed class GzipCompressor(int compressionLevel) : LibDeflateCompressor(compressionLevel)
{
    protected override nuint CompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output
    )
    {
        return libdeflate_gzip_compress(
            Compressor,
            MemoryMarshal.GetReference(input),
            (nuint)input.Length,
            ref MemoryMarshal.GetReference(output),
            (nuint)output.Length
        );
    }

    protected override nuint GetBoundCore(
        nuint inputLength
    )
    {
        return libdeflate_gzip_compress_bound(Compressor, inputLength);
    }
}