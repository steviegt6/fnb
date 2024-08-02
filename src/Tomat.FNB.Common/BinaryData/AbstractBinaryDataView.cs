using System.IO;

namespace Tomat.FNB.Common.BinaryData;

/// <summary>
///     An abstract implementation of <see cref="IBinaryDataView"/> that
///     implements basic handling of certain flags for optimizations and
///     boilerplate reduction.
/// </summary>
public abstract class AbstractBinaryDataView : IBinaryDataView
{
    public BinaryDataViewFlags Flags { get; set; }

    public abstract int Size { get; }

    IBinaryDataView IBinaryDataView.CompressDeflate()
    {
        return (Flags & BinaryDataViewFlags.CompressedDeflate) != 0 ? this : CompressDeflate();
    }

    protected abstract IBinaryDataView CompressDeflate();

    public abstract void Write(BinaryWriter writer);
}