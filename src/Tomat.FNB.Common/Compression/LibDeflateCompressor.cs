using System;

namespace Tomat.FNB.Common.Compression;

public abstract class LibDeflateCompressor : Compressor
{
    protected nint CompressorPtr { get; }

    protected LibDeflateCompressor(int compressionLevel)
    {
        if (compressionLevel is < 0 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(compressionLevel));
        }

        CompressorPtr = libdeflate_alloc_compressor(compressionLevel);
        if (CompressorPtr == nint.Zero)
        {
            throw new InvalidOperationException("Failed to allocate compressor");
        }
    }

    public override int Compress(ReadOnlySpan<byte> input, Span<byte> output)
    {
        return (int)CompressCore(input, output);
    }

    public override int GetBound(int inputLength)
    {
        return (int)GetBoundCore((nuint)inputLength);
    }

    protected abstract nuint CompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output
    );

    protected abstract nuint GetBoundCore(nuint inputLength);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            libdeflate_free_compressor(CompressorPtr);
        }
    }
}