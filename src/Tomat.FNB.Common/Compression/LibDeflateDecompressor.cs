using System;
using System.Buffers;

namespace Tomat.FNB.Common.Compression;

public abstract class LibDeflateDecompressor : Decompressor
{
    protected nint Decompressor { get; }

    protected LibDeflateDecompressor()
    {
        Decompressor = libdeflate_alloc_decompressor();
        if (Decompressor == nint.Zero)
        {
            throw new InvalidOperationException("Failed to allocate decompressor");
        }
    }

    public override OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        int                uncompressedSize
    )
    {
        return DecompressCore(input, output, (nuint)uncompressedSize);
    }

    public override OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out int            bytesWritten
    )
    {
        var status = DecompressCore(input, output, out var bytesWrittenActual);
        bytesWritten = (int)bytesWrittenActual;
        return status;
    }

    public override OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        int                uncompressedSize,
        out int            bytesRead
    )
    {
        var status = DecompressCore(input, output, (nuint)uncompressedSize, out var bytesReadActual);
        bytesRead = (int)bytesReadActual;
        return status;
    }

    public override OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out int            bytesWritten,
        out int            bytesRead
    )
    {
        var status = DecompressCore(input, output, out var bytesWrittenActual, out var bytesReadActual);
        bytesWritten = (int)bytesWrittenActual;
        bytesRead    = (int)bytesReadActual;
        return status;
    }

    protected abstract OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        nuint              uncompressedSize
    );

    protected abstract OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out nuint          bytesWritten
    );

    protected abstract OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        nuint              uncompressedSize,
        out nuint          bytesRead
    );

    protected abstract OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out nuint          bytesWritten,
        out nuint          bytesRead
    );

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            libdeflate_free_decompressor(Decompressor);
        }
    }
}