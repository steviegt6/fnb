using System.IO;

namespace Tomat.FNB.Common.BinaryData;

public sealed class ByteArrayBinaryDataView(byte[] data) : AbstractBinaryDataView
{
    public override int Size => Data.Length;

    private byte[] Data { get; } = data;

    protected override IBinaryDataView CompressDeflate()
    {
        
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(Data);
    }
}