using System;
using System.Runtime.InteropServices;

namespace Tomat.FNB.Common.Checksums;

public struct Adler32
{
    private bool initialized;
    private uint hash;

    public uint Hash
    {
        get
        {
            // Force Hash to be 1 if not initialized.  This means this value
            // will be 1 even if the struct was zeroed.
            if (!initialized)
            {
                hash        = 1;
                initialized = true;
            }

            return hash;
        }

        set => hash = value;
    }

    public void Append(ReadOnlySpan<byte> input)
    {
        Hash = Append(Hash, input);
    }

    public uint Compute(ReadOnlySpan<byte> input)
    {
        return Hash = Append(1, input);
    }

    private static uint Append(uint adler, ReadOnlySpan<byte> input)
    {
        return libdeflate_adler32(adler, MemoryMarshal.GetReference(input), (nuint)input.Length);
    }
}