using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TML.Files;

/// <summary>
///     Represents a .tmod archive file.
/// </summary>
public class TModFile
{
    #region Constants

    /// <summary>
    ///     The default minimum compression size for files; one kilobyte. If a file is smaller than this, it will not be compressed. <br />
    ///     The minimum compression size may be overridden in <see cref="AddFile"/>, in which case the same logic would apply to the passed value.
    /// </summary>
    public const uint DEFAULT_MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1 kilobyte

    /// <summary>
    ///     The default minimum compression tradeoff for files; 0.9f. If the compressed size of a file is not less than the uncompressed size multiplied by this value, the file will not be compressed. <br />
    ///     The compression tradeoff may be overridden in <see cref="AddFile"/>, in which case the same logic would apply to the passed value.
    /// </summary>
    public const float DEFAULT_MINIMUM_COMPRESSION_TRADEOFF = 0.9f;

    /// <summary>
    ///     The expected header for .tmod files; `TMOD`.
    /// </summary>
    public const string HEADER = "TMOD";

    #endregion

    #region Data

    /// <summary>
    ///     This .tmod file's header.
    /// </summary>
    public string Header { get; set; } = HEADER;

    /// <summary>
    ///     The version of tModLoader this .tmod file was packed with.
    /// </summary>
    public virtual string ModLoaderVersion { get; set; } = "";

    /// <summary>
    ///     The SHA-1 hash of the file entries in this .tmod file.
    /// </summary>
    public virtual byte[] Hash { get; set; } = Array.Empty<byte>();

    /// <summary>
    ///     The mod browser signature of this .tmod file. Goes unused in both 1.3 and 1.4, expected to be an array of empty bytes.
    /// </summary>
    public virtual byte[] Signature { get; set; } = Array.Empty<byte>();

    /// <summary>
    ///     This mod's internal name.
    /// </summary>
    public virtual string Name { get; set; } = "";

    /// <summary>
    ///     This mod's version. <br />
    ///     Must be parseable by <see cref="System.Version.Parse(string)"/>.
    /// </summary>
    public virtual string Version { get; set; } = "";

    /// <summary>
    ///     A collection of <see cref="TModFileEntry"/>s representing the files in this .tmod file.
    /// </summary>
    public virtual List<TModFileEntry> Entries { get; set; } = new();

    #endregion

    #region File adding

    /// <summary>
    ///     Adds the given <paramref name="file"/> to this .tmod file archive. Handles compression (see: <see cref="ShouldCompress"/>, <see cref="Compress"/>) using <paramref name="minCompSize"/> and <paramref name="inCompTradeoff"/>.
    /// </summary>
    /// <param name="file">The file to pack into this archive.</param>
    /// <param name="minCompSize">The minimum compression size for this file. See <see cref="DEFAULT_MINIMUM_COMPRESSION_SIZE"/> for more details.</param>
    /// <param name="inCompTradeoff">The minimum compression tradeoff for this file. See <see cref="DEFAULT_MINIMUM_COMPRESSION_TRADEOFF"/> for more details.</param>
    /// <seealso cref="ShouldCompress"/>
    /// <seealso cref="Compress"/>
    /// <seealso cref="DEFAULT_MINIMUM_COMPRESSION_SIZE"/>
    /// <seealso cref="DEFAULT_MINIMUM_COMPRESSION_TRADEOFF"/>
    public virtual void AddFile(
        TModFileData file,
        uint minCompSize = DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float inCompTradeoff = DEFAULT_MINIMUM_COMPRESSION_TRADEOFF
    ) {
        file.Path = file.Path.Trim().Replace('\\', '/');

        int size = file.Data.Length;
        if (size > minCompSize && ShouldCompress(file)) Compress(file, size, inCompTradeoff);

        Entries.Add(new TModFileEntry
        {
            Path = file.Path,
            Offset = -1,
            Length = size,
            CompressedLength = file.Data.Length,
            Data = file.Data
        });
    }

    /// <summary>
    ///     Whether a <see cref="TModFileData"/>'s <see cref="TModFileData.Data"/> should be compressed when added through <see cref="AddFile"/>. <br />
    ///     By default, <c>.png</c>, <c>.png</c>, and <c>.png</c> files are not compressed.
    /// </summary>
    /// <param name="fileData">The file data to check for compression.</param>
    /// <returns>Whether compression should occur.</returns>
    protected virtual bool ShouldCompress(TModFileData fileData) {
        return new[] {".png", ".mp3", ".ogg"}.Contains(Path.GetExtension(fileData.Path));
    }

    /// <summary>
    ///     Compresses the <see cref="TModFileData.Data"/> from the passed <paramref name="file"/> using a <see cref="DeflateStream"/>.
    /// </summary>
    /// <param name="file">The file to compress.</param>
    /// <param name="realSize">The real size of this file, used to check against the <paramref name="tradeoff"/>.</param>
    /// <param name="tradeoff">The minimum compression tradeoff for this file. See <see cref="DEFAULT_MINIMUM_COMPRESSION_TRADEOFF"/> for more details.</param>
    protected virtual void Compress(TModFileData file, int realSize, float tradeoff) {
        using MemoryStream ms = new(file.Data.Length);
        using (DeflateStream ds = new(ms, CompressionMode.Compress)) ds.Write(file.Data, 0, file.Data.Length);
        byte[] com = ms.ToArray();
        if (com.Length < realSize * tradeoff) file.Data = com;
    }

    #endregion
}