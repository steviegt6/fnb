using System.Collections.Generic;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A <c>.tmod</c> file archive.
/// </summary>
/// <remarks>
///     <c>.tmod</c> files are designed to pack an entire mod, meaning much of
///     its metadata is designed around that.  Many properties pertain
///     specifically to the mod.
/// </remarks>
public interface ITmodFile
{
    /// <summary>
    ///     The version of tModLoader that this archive was created with.
    /// </summary>
    /// <remarks>
    ///     Specifically, this is intended to specify what version of
    ///     tModLoader the mod within targets.
    ///     <br />
    ///     It is best to use the appropriate value within your own code using
    ///     a known tModLoader version.
    ///     <br />
    ///     The <c>.tmod</c> file format itself may change with updates, so
    ///     this value is also used to handle multi-versioning.  The format
    ///     does not change with every update.
    /// </remarks>
    string ModLoaderVersion { get; set; }

    /// <summary>
    ///     The internal name of the mod contained within the archive.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     The <see cref="System.Version"/>-compatible version of the mod
    ///     contained within the archive.
    /// </summary>
    string Version { get; set; }

    /// <summary>
    ///     The file entries within the <c>.tmod</c> archive.
    /// </summary>
    IReadOnlyCollection<TmodFileEntry> Entries { get; }

    TmodFileEntry this[string path] { get; }

    /// <summary>
    ///     Adds a file to the <c>.tmod</c> archive.
    /// </summary>
    /// <param name="file">The file data to add, including the path.</param>
    /// <param name="minimumCompressionSize">
    ///     The minimum size of the file to compress.
    /// </param>
    /// <param name="minimumCompressionTradeoff">
    ///     The minimum compression tradeoff for compression.
    /// </param>
    void AddFile(
        TmodFileData file,
        uint         minimumCompressionSize     = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float        minimumCompressionTradeoff = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_TRADEOFF
    );

    /// <summary>
    ///     Removes a file from the <c>.tmod</c> archive.
    /// </summary>
    /// <param name="path">The pah of the file to remove.</param>
    /// <returns>Whether the file was found and removed.</returns>
    bool RemoveFile(string path);
}