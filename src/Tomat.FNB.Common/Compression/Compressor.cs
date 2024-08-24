using System;
using System.Buffers;

using CommunityToolkit.HighPerformance.Buffers;

namespace Tomat.FNB.Common.Compression;

public abstract class Compressor : IDisposable
{
    protected nint CompressorPtr { get; }

    private bool disposed;

    protected Compressor(int compressionLevel)
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

    ~Compressor()
    {
        Dispose(disposing: false);
    }

    public IMemoryOwner<byte>? Compress(
        ReadOnlySpan<byte> input,
        bool               useUpperBound = false
    )
    {
        DisposedGuard();
        {
            var output = MemoryOwner<byte>.Allocate(useUpperBound ? GetBound(input.Length) : input.Length);
            try
            {
                var bytesWritten = CompressCore(input, output.Span);
                if (bytesWritten != nuint.Zero)
                {
                    return output[..(int)bytesWritten];
                }

                output.Dispose();
                return null;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }
    }

    public int Compress(
        ReadOnlySpan<byte> input,
        Span<byte>         output
    )
    {
        DisposedGuard();
        {
            return (int)CompressCore(input, output);
        }
    }

    protected abstract nuint CompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output
    );

    protected abstract nuint GetBoundCore(
        nuint inputLength
    );

    private int GetBound(
        int inputLength
    )
    {
        DisposedGuard();
        {
            return (int)GetBoundCore((nuint)inputLength);
        }
    }

    private void DisposedGuard()
    {
        if (!disposed)
        {
            return;
        }

        throw new ObjectDisposedException(nameof(Compressor));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        libdeflate_free_compressor(CompressorPtr);
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}