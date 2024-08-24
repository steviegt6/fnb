using System;
using System.Buffers;

using CommunityToolkit.HighPerformance.Buffers;

namespace Tomat.FNB.Common.Compression;

public abstract class Compressor : IDisposable
{
    public IMemoryOwner<byte>? Compress(
        ReadOnlySpan<byte> input,
        bool               useUpperBound = false
    )
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

    public int Compress(
        ReadOnlySpan<byte> input,
        Span<byte>         output
    )
    {
        return (int)CompressCore(input, output);
    }
    
    public int GetBound(
        int inputLength
    )
    {
        return (int)GetBoundCore((nuint)inputLength);
    }

    protected abstract nuint CompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output
    );

    protected abstract nuint GetBoundCore(
        nuint inputLength
    );

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}