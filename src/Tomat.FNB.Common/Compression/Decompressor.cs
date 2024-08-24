using System;
using System.Buffers;

using CommunityToolkit.HighPerformance.Buffers;

namespace Tomat.FNB.Common.Compression;

public abstract class Decompressor : IDisposable
{
    public abstract OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        int                uncompressedSize
    );

    public abstract OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out int            bytesWritten
    );

    public abstract OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        int                uncompressedSize,
        out int            bytesRead
    );

    public abstract OperationStatus Decompress(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out int            bytesWritten,
        out int            bytesRead
    );

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public static class DecompressorExtensions
{
    public static OperationStatus Decompress(
        this Decompressor       @this,
        ReadOnlySpan<byte>      input,
        int                     uncompressedSize,
        out IMemoryOwner<byte>? outputOwner,
        out int                 bytesRead
    )
    {
        var output = MemoryOwner<byte>.Allocate(uncompressedSize);
        try
        {
            var status = @this.Decompress(input, output.Span, uncompressedSize, out var inBytesCount);
            switch (status)
            {
                case OperationStatus.Done:
                    outputOwner = output;
                    bytesRead   = inBytesCount;
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

    public static OperationStatus Decompress(
        this Decompressor       @this,
        ReadOnlySpan<byte>      input,
        int                     uncompressedSize,
        out IMemoryOwner<byte>? outputOwner
    )
    {
        var output = MemoryOwner<byte>.Allocate(uncompressedSize);
        try
        {
            var status = @this.Decompress(input, output.Span, uncompressedSize);
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

    public static OperationStatus Decompress(
        this Decompressor  @this,
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out int            bytesWritten,
        out int            bytesRead
    )
    {
        var status = @this.Decompress(input, output, out var outBytesCount, out var inBytesCount);
        switch (status)
        {
            case OperationStatus.Done:
                bytesWritten = outBytesCount;
                bytesRead    = inBytesCount;
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

    public static OperationStatus Decompress(
        this Decompressor  @this,
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out int            bytesWritten
    )
    {
        var status = @this.Decompress(input, output, out var outBytesCount);
        switch (status)
        {
            case OperationStatus.Done:
                bytesWritten = outBytesCount;
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