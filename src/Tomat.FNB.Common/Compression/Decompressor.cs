using System;
using System.Buffers;

using CommunityToolkit.HighPerformance.Buffers;

namespace Tomat.FNB.Common.Compression;

public abstract class Decompressor : IDisposable
{
    protected nint DecompressorPtr { get; }

    private bool disposed;

    protected Decompressor()
    {
        DecompressorPtr = libdeflate_alloc_decompressor();
        if (DecompressorPtr == nint.Zero)
        {
            throw new InvalidOperationException("Failed to allocate decompressor");
        }
    }

    ~Decompressor()
    {
        Dispose(disposing: false);
    }

    public OperationStatus Decompress(
        ReadOnlySpan<byte>      input,
        int                     uncompressedSize,
        out IMemoryOwner<byte>? outputOwner,
        out int                 bytesRead
    )
    {
        DisposedGuard();
        {
            var output = MemoryOwner<byte>.Allocate(uncompressedSize);
            try
            {
                var status = DecompressCore(input, output.Span, (nuint)uncompressedSize, out var inBytesCount);
                switch (status)
                {
                    case OperationStatus.Done:
                        outputOwner = output;
                        bytesRead   = (int)inBytesCount;
                        return status;

                    case OperationStatus.NeedMoreData:
                    case OperationStatus.DestinationTooSmall:
                    case OperationStatus.InvalidData:
                    default:
                        output.Dispose();
                        outputOwner = null;
                        bytesRead   = 0;
                        return status;
                }
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }
    }

    public OperationStatus Decompress(
        ReadOnlySpan<byte>      input,
        int                     uncompressedSize,
        out IMemoryOwner<byte>? outputOwner
    )
    {
        DisposedGuard();
        {
            var output = MemoryOwner<byte>.Allocate(uncompressedSize);
            try
            {
                var status = DecompressCore(input, output.Span, uncompressedSize: (nuint)uncompressedSize);
                switch (status)
                {
                    case OperationStatus.Done:
                        outputOwner = output;
                        return status;

                    case OperationStatus.NeedMoreData:
                    case OperationStatus.DestinationTooSmall:
                    case OperationStatus.InvalidData:
                    default:
                        output.Dispose();
                        outputOwner = null;
                        return status;
                }
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }
    }

    public OperationStatus Decompress(ReadOnlySpan<byte> input, Span<byte> output, out int bytesWritten, out int bytesRead)
    {
        DisposedGuard();
        {
            var status = DecompressCore(input, output, out var outBytesCount, out var inBytesCount);
            switch (status)
            {
                case OperationStatus.Done:
                    bytesWritten = (int)outBytesCount;
                    bytesRead    = (int)inBytesCount;
                    return status;

                case OperationStatus.NeedMoreData:
                case OperationStatus.DestinationTooSmall:
                case OperationStatus.InvalidData:
                default:
                    bytesWritten = 0;
                    bytesRead    = 0;
                    return status;
            }
        }
    }

    public OperationStatus Decompress(ReadOnlySpan<byte> input, Span<byte> output, out int bytesWritten)
    {
        DisposedGuard();
        {
            var status = DecompressCore(input, output, out var outBytesCount);
            switch (status)
            {
                case OperationStatus.Done:
                    bytesWritten = (int)outBytesCount;
                    return status;

                case OperationStatus.NeedMoreData:
                case OperationStatus.DestinationTooSmall:
                case OperationStatus.InvalidData:
                default:
                    bytesWritten = 0;
                    return status;
            }
        }
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

    private void DisposedGuard()
    {
        if (!disposed)
        {
            return;
        }

        throw new ObjectDisposedException(nameof(Decompressor));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        libdeflate_free_decompressor(DecompressorPtr);
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}