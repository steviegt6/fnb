using System;
using System.Buffers;

using CommunityToolkit.HighPerformance.Buffers;

namespace Tomat.FNB.Common.Compression;

public abstract class Compressor : IDisposable
{
    public abstract int Compress(ReadOnlySpan<byte> input, Span<byte> output);

    public abstract int GetBound(int inputLength);

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public static class CompressorExtensions
{
    public static IMemoryOwner<byte>? Compress(
        this Compressor    @this,
        ReadOnlySpan<byte> input,
        bool               useUpperBound = false
    )
    {
        var output = MemoryOwner<byte>.Allocate(useUpperBound ? @this.GetBound(input.Length) : input.Length);
        try
        {
            var bytesWritten = @this.Compress(input, output.Span);
            if (bytesWritten != 0)
            {
                return output[..bytesWritten];
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