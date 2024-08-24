using System;

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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            libdeflate_free_decompressor(Decompressor);
        }
    }
}