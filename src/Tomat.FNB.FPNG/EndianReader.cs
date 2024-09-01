global using static Tomat.FNB.FPNG.EndianReader;

using System;
using System.Runtime.CompilerServices;

namespace Tomat.FNB.FPNG;

internal static unsafe class EndianReader
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadLe32(uint* data)
    {
        // PERF: JIT compiler should be smart enough to optimize out branches at
        //       runtime and use the correct path based on endianness (because
        //       IsLittleEndian is a static readonly field).
        if (BitConverter.IsLittleEndian)
        {
            return *data;
        }

        return Swap32(*data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadBe32(uint* data)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Swap32(*data);
        }

        return *data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Swap32(uint x)
    {
        return (x >> 24)
             | ((x >> 8) & 0xFF00)
             | ((x << 8) & 0xFF0000)
             | (x << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Swap64(ulong x)
    {
        return ((ulong)Swap32((uint)x) << 32) | Swap32((uint)(x >> 32));
    }
}