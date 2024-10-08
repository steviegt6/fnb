﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Tomat.FNB.Deflate;

namespace Tomat.FNB.Common.Compression;

public sealed class DeflateDecompressor : LibDeflateDecompressor
{
    protected override OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        nuint              uncompressedSize
    )
    {
        return libdeflate_deflate_decompress(
            Decompressor,
            MemoryMarshal.GetReference(input),
            (nuint)input.Length,
            ref MemoryMarshal.GetReference(output),
            uncompressedSize,
            outBytesCountActual: out Unsafe.NullRef<nuint>()
        ).ToStatus();
    }

    protected override OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out nuint          bytesWritten
    )
    {
        return libdeflate_deflate_decompress(
            Decompressor,
            MemoryMarshal.GetReference(input),
            (nuint)input.Length,
            ref MemoryMarshal.GetReference(output),
            (nuint)output.Length,
            out bytesWritten
        ).ToStatus();
    }

    protected override OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        nuint              uncompressedSize,
        out nuint          bytesRead
    )
    {
        return libdeflate_deflate_decompress_ex(
            Decompressor,
            MemoryMarshal.GetReference(input),
            (nuint)input.Length,
            ref MemoryMarshal.GetReference(output),
            uncompressedSize,
            out bytesRead,
            out Unsafe.NullRef<nuint>()
        ).ToStatus();
    }

    protected override OperationStatus DecompressCore(
        ReadOnlySpan<byte> input,
        Span<byte>         output,
        out nuint          bytesWritten,
        out nuint          bytesRead
    )
    {
        return libdeflate_deflate_decompress_ex(
            Decompressor,
            MemoryMarshal.GetReference(input),
            (nuint)input.Length,
            ref MemoryMarshal.GetReference(output),
            (nuint)output.Length,
            out bytesRead,
            out bytesWritten
        ).ToStatus();
    }
}