using Tomat.FNB.Common.BinaryData;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A descriptive representation of a file within a <c>.tmod</c> archive.
///     <br />
///     Represents an actual entry.
/// </summary>
/// <param name="Path">The path of the file within the archive.</param>
/// <param name="Offset">The offset of the file within the archive.</param>
/// <param name="Length">The length of the file when uncompressed.</param>
/// <param name="CompressedLength">The compressed file length.</param>
/// <param name="Data">The optionally compressed file data.</param>
public readonly record struct TmodFileEntry(
    string           Path,
    int              Offset,
    int              Length,
    int              CompressedLength,
    IDataView? Data
);