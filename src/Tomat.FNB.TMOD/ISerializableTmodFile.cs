using System.Collections.Generic;
using System.IO;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A <c>.tmod</c> file which is ready for serialization. The direct output
///     of deserialization as well.
/// </summary>
public interface ISerializableTmodFile : IReadOnlyTmodFile
{
    /// <summary>
    ///     A file entry within a serializable <c>.tmod</c> file, containing all
    ///     information about how an entry is stored within a file.
    /// </summary>
    public readonly struct FileEntry
    {
        /// <summary>
        ///     The offset of this entry within the <c>.tmod</c> file.
        /// </summary>
        /// <remarks>
        ///     This is only knowable for non-legacy versions of the
        ///     <c>.tmod</c> file format. Thus, it is possible to check whether
        ///     this entry belongs to a legacy file by checking if this value is
        ///     zero.
        /// </remarks>
        public int Offset { get; init; }

        /// <summary>
        ///     The real (uncompressed) length of the entry.
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        ///     The compressed length of the entry.
        /// </summary>
        /// <remarks>
        ///     If the entry is not compressed, this value will be equal to
        ///     the <see cref="Length"/>.
        ///     <br />
        ///     If this entry belongs to a legacy <c>.tmod</c> file, this value
        ///     will always be zero.
        /// </remarks>
        public int CompressedLength { get; init; }

        /// <summary>
        ///     The byte data of the entry.
        /// </summary>
        /// <remarks>
        ///     Not guaranteed to be initialized until the entire file is read.
        /// </remarks>
        public byte[]? Data { get; init; }
    }

    /// <summary>
    ///     A hashmap (path -> file entry) of all the files within this
    ///     <c>.tmod</c> file.
    /// </summary>
    new IReadOnlyDictionary<string, FileEntry> Entries { get; }

    /// <summary>
    ///     Retrieves the file data for the given path.
    /// </summary>
    /// <param name="path">
    ///     The path to the file within the <c>.tmod</c> file.
    /// </param>
    new FileEntry this[string path] { get; }

    /// <summary>
    ///     Writes the <c>.tmod</c> file to the given stream.
    /// </summary>
    /// <param name="stream">
    ///     The stream to write the <c>.tmod</c> file to.
    /// </param>
    void Write(Stream stream);
}