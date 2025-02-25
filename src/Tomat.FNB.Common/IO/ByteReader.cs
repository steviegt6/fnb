using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tomat.FNB.Common.IO;

public readonly record struct ByteReader(
    Stream Stream,
    bool   OwnsStream = false
) : IDisposable
{
    private readonly BinaryReader reader = new(
        Stream,
        encoding: Encoding.UTF8,
        leaveOpen: true
    );

#region Primitives
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte U8()
    {
        return reader.ReadByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte S8()
    {
        return reader.ReadSByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort U16()
    {
        return reader.ReadUInt16();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short S16()
    {
        return reader.ReadInt16();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint U32()
    {
        return reader.ReadUInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int S32()
    {
        return reader.ReadInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong U64()
    {
        return reader.ReadUInt64();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long S64()
    {
        return reader.ReadInt64();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float F32()
    {
        return reader.ReadSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double F64()
    {
        return reader.ReadDouble();
    }

    public byte[] Bytes(int count)
    {
        return reader.ReadBytes(count);
    }

    public int Span(Span<byte> span)
    {
        return reader.Read(span);
    }
#endregion

    public string NetString()
    {
        return reader.ReadString();
    }

    public void Dispose()
    {
        reader.Dispose();

        if (OwnsStream)
        {
            Stream.Dispose();
        }
    }
}