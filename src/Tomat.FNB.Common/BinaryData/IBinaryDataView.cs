using System.IO;

namespace Tomat.FNB.Common.BinaryData;

/// <summary>
///     An abstraction over traditional methods of holding onto binary data
///     (byte arrays, streams, managed and native pointers, etc.).
/// </summary>
/// <remarks>
///     This is intended for leverage in memory- and performance-critical or
///     intensive code where minimizing conversion between representations is
///     preferable.
/// </remarks>
public interface IBinaryDataView
{
    /// <summary>
    ///     Flags describing this data view.
    /// </summary>
    BinaryDataViewFlags Flags { get; set; }

    /// <summary>
    ///     The size of the data in bytes.
    /// </summary>
    int Size { get; }

    /// <summary>
    ///     Compresses the data using the DEFLATE algorithm.
    /// </summary>
    IBinaryDataView CompressDeflate();

    /// <summary>
    ///     Writes the data to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    void Write(BinaryWriter writer);
}