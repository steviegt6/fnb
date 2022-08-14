namespace TML.Files.Abstractions
{
    /// <summary>
    ///     Represents a file entry in an <see cref="IModFile"/>.
    /// </summary>
    public interface IModFileEntry
    {
        /// <summary>
        ///     The file name, including the path.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The byte offset of the file entry in the <c>.tmod</c> file.
        /// </summary>
        int Offset { get; }

        /// <summary>
        ///     The real length of the file.
        /// </summary>
        int Length { get; }

        /// <summary>
        ///     The compressed length of the file, if it is compressed.
        /// </summary>
        int CompressedLength { get; }

        /// <summary>
        ///     The compressed file bytes.
        /// </summary>
        byte[]? CachedBytes { get; }
    }
}