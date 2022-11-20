using TML.Files.Extraction;

namespace TML.Files;

/// <summary>
///     Simple data structure used to record a file within a <see cref="TModFile"/>. <br />
///     This is how a file should be represented when being added to a <see cref="TModFile"/> through <see cref="TModFile.AddFile"/>. <br />
///     Additionally, an instance of this record is produced when extracting a <see cref="TModFileEntry"/> through am <see cref="IFileExtractor"/> or through <see cref="TModFileExtractor.Extract"/>.
/// </summary>
/// <param name="Path">The file's path, relative to the <see cref="TModFile"/>.</param>
/// <param name="Data">The file's data in a raw array of bytes.</param>
public record TModFileData(string Path, byte[] Data)
{
    /// <summary>
    ///     The file's path, relative to the <see cref="TModFile"/>.
    /// </summary>
    public string Path { get; set; } = Path;

    /// <summary>
    ///     The file's data in a raw array of bytes.
    /// </summary>
    public byte[] Data { get; set; } = Data;
}