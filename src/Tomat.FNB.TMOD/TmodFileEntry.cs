namespace Tomat.FNB.TMOD;

/// <summary>
///     Represents a <c>.tmod</c> file entry.
/// </summary>
/// <param name="Path">
///     The path of this file entry, serving as a unique name.
/// </param>
/// <param name="Length">The actual length of the stored file.</param>
/// <param name="CompressedLength">
///     The compressed length of the file, if applicable.
/// </param>
/// <param name="StreamOffset">
///     The offset of the file data in the stream this entry was read from.
/// </param>
public readonly record struct TmodFileEntry(
    string Path,
    int    Length,
    int    CompressedLength,
    long   StreamOffset
)
{
    /// <summary>
    ///     Whether this file entry is compressed and needs to be decompressed.
    /// </summary>
    public bool IsCompressed => Length != CompressedLength;

    /// <summary>
    ///     Whether this entry has a known stream offset to read from.
    /// </summary>
    public bool Readable => StreamOffset > 0;
}