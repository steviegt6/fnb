using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Tomat.FNB.Common;
using U8;

namespace Tomat.FNB.TMOD;

/// <summary>
///     Minimal information describing the data of a file within a <c>.tmod</c>
///     archive.
/// </summary>
/// <param name="Path">The path of the file within the archive.</param>
/// <param name="Data">The data of the file within the archive.</param>
public readonly record struct TmodFileData(string Path, IBinaryDataView Data);

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
public readonly record struct TmodFileEntry(string Path, int Offset, int Length, int CompressedLength, IBinaryDataView? Data);

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
    U8String ModLoaderVersion { get; set; }

    /// <summary>
    ///     The internal name of the mod contained within the archive.
    /// </summary>
    U8String Name { get; set; }

    /// <summary>
    ///     The <see cref="System.Version"/>-compatible version of the mod
    ///     contained within the archive.
    /// </summary>
    U8String Version { get; set; }

    /// <summary>
    ///     The file entries within the <c>.tmod</c> archive.
    /// </summary>
    IReadOnlyCollection<TmodFileEntry> Entries { get; }

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
    void AddFile(TmodFileData file, uint minimumCompressionSize = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_SIZE, float minimumCompressionTradeoff = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_TRADEOFF);

    /// <summary>
    ///     Removes a file from the <c>.tmod</c> archive.
    /// </summary>
    /// <param name="path">The pah of the file to remove.</param>
    /// <returns>Whether the file was found and removed.</returns>
    bool RemoveFile(string path);

    /// <summary>
    ///     Attempts to write the <c>.tmod</c> archive to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <returns>Whether writing was a success.</returns>
    bool TryWrite(Stream stream);
}

#region TmodFile
public sealed class TmodFile : ITmodFile
{
    private const char dirty_separator = '\\';
    private const char clean_separator = '/';

    public U8String ModLoaderVersion { get; set; }

    public U8String Name { get; set; }

    public U8String Version { get; set; }

    public IReadOnlyCollection<TmodFileEntry> Entries => entries.Values;

    // These files are already compressed.
    private static readonly string[] extensions_to_not_compress = [".png", ".mp3", ".ogg"];

    private readonly Dictionary<string, TmodFileEntry> entries;

    public TmodFile(U8String modLoaderVersion, U8String name, U8String version, Dictionary<TmodFileEntry> entries)
    {
        ModLoaderVersion = modLoaderVersion;
        Name = name;
        Version = version;
        this.entries = entries;
    }

    public void AddFile(TmodFileData file, uint minimumCompressionSize = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_SIZE, float minimumCompressionTradeoff = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_TRADEOFF)
    {
        file = file with
        {
            Path = SanitizePath(file.Path)
        };

        var size = file.Data.Size;
        if (EligibleForCompression(file, minimumCompressionSize))
        {
            Compress(ref file, size, minimumCompressionTradeoff);
        }

        entries.Add(file.Path, new TmodFileEntry(file.Path, 0, size, size, file.Data));
    }

    public bool RemoveFile(string path)
    {
        throw new NotImplementedException();
    }

    public bool TryWrite(Stream stream)
    {
        throw new NotImplementedException();
    }

    private static string SanitizePath(string path)
    {
        return path.Trim().Replace(dirty_separator, clean_separator);
    }

    private static bool EligibleForCompression(TmodFileData file, uint minCompSize)
    {
        return file.Data.Size >= minCompSize && !extensions_to_not_compress.Contains(Path.GetExtension(file.Path));
    }

    private static void Compress(ref TmodFileData file, int realSize, float tradeoff)
    {
        var data = file.Data;
        var compressed = data.CompressDeflate();

        if (compressed.Size < realSize * tradeoff)
        {
            file = file with
            {
                Data = compressed,
            };
        }
    }
}
#endregion

#region ReadOnlyTmodFile
/// <summary>
///     A read-only view into a <c>.tmod</c> file archive.
/// </summary>
public sealed class ReadOnlyTmodFile : ITmodFile
{
    U8String ITmodFile.ModLoaderVersion
    {
        get => tmodFile.ModLoaderVersion;
        set => throw new InvalidOperationException("Cannot change the mod loader version of a read-only `.tmod` file!");
    }

    U8String ITmodFile.Name
    {
        get => tmodFile.Name;
        set => throw new InvalidOperationException("Cannot change the name of a read-only `.tmod` file!");
    }

    U8String ITmodFile.Version
    {
        get => tmodFile.Version;
        set => throw new InvalidOperationException("Cannot change the version of a read-only `.tmod` file!");
    }

    IReadOnlyCollection<TmodFileEntry> ITmodFile.Entries => tmodFile.Entries;

    private readonly ITmodFile tmodFile;

    public ReadOnlyTmodFile(ITmodFile tmodFile)
    {
        this.tmodFile = tmodFile;
    }

    void ITmodFile.AddFile(TmodFileData file, uint minimumCompressionSize, float minimumCompressionTradeoff)
    {
        throw new InvalidOperationException("Cannot add files to a read-only `.tmod` file!");
    }

    bool ITmodFile.RemoveFile(string path)
    {
        throw new InvalidOperationException("Cannot remove files from a read-only `.tmod` file!");
    }

    bool ITmodFile.TryWrite(Stream stream)
    {
        return tmodFile.TryWrite(stream);
    }
}
#endregion
