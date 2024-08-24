using System;
using System.Runtime.InteropServices;

namespace Tomat.FNB.Common.Checksums;

public struct Crc32
{
    public uint Hash { get; set; }

    public void Append(ReadOnlySpan<byte> input)
    {
        Hash = Append(Hash, input);
    }

    public uint Compute(ReadOnlySpan<byte> input)
    {
        return Hash = Append(0, input);
    }

    private static uint Append(uint crc, ReadOnlySpan<byte> input)
    {
        return libdeflate_crc32(crc, MemoryMarshal.GetReference(input), (nuint)input.Length);
    }
}