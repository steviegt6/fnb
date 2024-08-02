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
public interface IDataView
{
    /// <summary>
    ///     The size of the data in bytes.
    /// </summary>
    long Size { get; }

    IDataView CompressDeflate();

    IDataView DecompressDeflate();

    /// <summary>
    ///     Writes the data to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    void Write(BinaryWriter writer);
}