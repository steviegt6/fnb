namespace TML.Files;

/// <summary>
///     Represents a file located within a <see cref="TModFile"/>. Produced from a <see cref="TModFileData"/> record through <see cref="TModFile.AddFile"/>. <br />
///     Contains data about file compression and the internal data offset in the <see cref="TModFile"/>.
/// </summary>
public class TModFileEntry
{
    /// <summary>
    ///     The file's path, relative to the <see cref="TModFile"/>.
    /// </summary>
    public virtual string Path { get; set; } = "";

    /// <summary>
    ///     The offset within the <see cref="TModFile"/>.
    /// </summary>
    public virtual int Offset { get; set; }

    /// <summary>
    ///     The uncompressed length of this file.
    /// </summary>
    public virtual int Length { get; set; }

    /// <summary>
    ///     The compressed length of this file.
    /// </summary>
    public virtual int CompressedLength { get; set; }

    /// <summary>
    ///     The actual data stored within this file, which is compressed if <see cref="CompressedLength"/> is less than <see cref="Length"/>.
    /// </summary>
    public virtual byte[]? Data { get; set; }
}