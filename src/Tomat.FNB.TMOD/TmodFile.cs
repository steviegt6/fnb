using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

using Tomat.FNB.Common.IO;
using Tomat.FNB.Common.IO.Compression;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A partially-deserialized <c>.tmod</c> file, containing header data and
///     known entries, as well as APIs for processing the data labeled by those
///     entries.
/// </summary>
/// <remarks>
///     This class is designed to be immutable when it comes to editing data (it
///     may mutate data internally for caching purposes).  If you want to build
///     a new <c>.tmod</c> file, see <see cref="TmodFileBuilder"/>.
/// </remarks>
public sealed class TmodFile : IDisposable
{
    /// <summary>
    ///     Represents a <c>.tmod</c> file entry.
    /// </summary>
    /// <param name="UncompressedLength">The length of the stored file.</param>
    /// <param name="CompressedLength">
    ///     The compressed length of the file, if applicable.  If this file is
    ///     not compressed, then this will be equal to
    ///     <see cref="UncompressedLength"/>.
    /// </param>
    /// <param name="StreamOffset">
    ///     The offset of the file data in the stream this entry was read from.
    /// </param>
    private readonly record struct Entry(
        int  UncompressedLength,
        int  CompressedLength,
        long StreamOffset
    )
    {
        /// <summary>
        ///     Whether this file entry is compressed and needs to be
        ///     decompressed.
        /// </summary>
        public bool IsCompressed => UncompressedLength != CompressedLength;

        /// <summary>
        ///     Whether this entry has a known stream offset to read from.
        /// </summary>
        /// <remarks>
        ///     It should always be true, but during deserialization entries
        ///     without a stream offset may be initialized.
        /// </remarks>
        public bool Readable => StreamOffset > 0;
    }

#region Constants
    /// <summary>
    ///     The default size needed to be met for a file to be compressed.
    /// </summary>
    public const long DEFAULT_MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1 KiB

    /// <summary>
    ///     The default tradeoff needed to be met for a file to be compressed.
    /// </summary>
    public const float DEFAULT_MINIMUM_COMPRESSION_TRADEOFF = 0.9f;

    /// <summary>
    ///     The version milestone for which the new compression format is
    ///     implemented, which compresses individual byte chunks instead of the
    ///     entire file table.
    /// </summary>
    private static readonly Version new_format_milestone = new(0, 11, 0, 0);

    /// <summary>
    ///     The magic header that denotes a <c>.tmod</c> file.
    /// </summary>
    public const int TMOD_MAGIC_HEADER = 0x444F4D54; // "TMOD"

    /// <summary>
    ///     The length of the <c>.tmod</c> hash part.
    /// </summary>
    public const int HASH_LENGTH = 20;

    /// <summary>
    ///     The length of the <c>.tmod</c> signature part.
    /// </summary>
    public const int SIGNATURE_LENGTH = 256;

    private const int stack_alloc_byte_threshold = (1 << 20) / 2;
#endregion

    private static readonly LibDeflateDecompressor decompressor = new();

    /// <summary>
    ///     The version of tModLoader this <c>.tmod</c> file was created from.
    ///     <br />
    ///     Obviously, tools like <b>fnb</b> mean the file may not have been created
    ///     through tModLoader, so it more realistically indicates the version the
    ///     mod is intended to be run under.
    /// </summary>
    public string TmlVersion { get; }

    /// <summary>
    ///     The uniquely-identifiable, internal name of the mod.  Distinctly not the
    ///     display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The version of the mod, which must be formatted as a
    ///     <see cref="System.Version.Parse(string)"/>-compatible string.
    /// </summary>
    public string Version { get; }

    private readonly Stream seekableStream;
    private readonly Stream readableStream;
    private readonly bool   ownsStreams;

    private readonly Dictionary<string, Entry>  entries;
    private readonly Dictionary<string, byte[]> readFileCache = [];

    /// <summary>
    ///     The names of every file in this archive.
    /// </summary>
    public IEnumerable<string> FileNames => entries.Keys;

    private TmodFile(
        Stream                    seekableStream,
        Stream                    readableStream,
        bool                      ownsStreams,
        string                    tmlVersion,
        string                    name,
        string                    version,
        Dictionary<string, Entry> entries
    )
    {
        this.seekableStream = seekableStream;
        this.readableStream = readableStream;
        this.ownsStreams    = ownsStreams;
        TmlVersion          = tmlVersion;
        Name                = name;
        Version             = version;
        this.entries        = entries;
    }

    /// <summary>
    ///     Whether the file at the given <paramref name="path"/> exists.
    /// </summary>
    /// <param name="path">The file path.</param>
    public bool FileExists(string path)
    {
        return entries.ContainsKey(path);
    }

    /// <summary>
    ///     Reads the file from the stream and caches it, returning the read
    ///     bytes.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The read file bytes.</returns>
    /// <remarks>
    ///     It is recommended to avoid this if possible, instead preferring
    ///     <see cref="ProcessFiles"/> to minimize unnecessary allocations.
    /// </remarks>
    /// <seealso cref="ProcessFiles"/>
    public byte[] GetFileBytes(string path)
    {
        if (readFileCache.TryGetValue(path, out var bytes))
        {
            return bytes;
        }

        var entry = entries[path];
        seekableStream.Position = entry.StreamOffset;

        bytes = new byte[entry.UncompressedLength];

        if (entry.IsCompressed)
        {
            var compressedBytes = entry.CompressedLength <= stack_alloc_byte_threshold
                ? stackalloc byte[entry.CompressedLength]
                : new byte[entry.CompressedLength];

            if (readableStream.Read(compressedBytes) != compressedBytes.Length)
            {
                throw new IOException($"Failed to read bytes for entry {path}");
            }

            if (!Decompress(compressedBytes, new Span<byte>(bytes)))
            {
                throw new IOException($"Failed to decompress bytes for entry {path}");
            }

            return readFileCache[path] = bytes;
        }

        if (readableStream.Read(bytes, 0, entry.UncompressedLength) != entry.UncompressedLength)
        {
            throw new IOException($"Failed to read bytes for entry {path}");
        }

        return readFileCache[path] = bytes;
    }

    /// <summary>
    ///     Operates over every file in a multithreaded fashion, minimizing
    ///     allocations of new byte arrays.
    /// </summary>
    public void ProcessFiles() { }

    public void Dispose()
    {
        if (!ownsStreams)
        {
            return;
        }

        // For now, we do not need to call ::Dispose() on SeekableStream since
        // we're assuming ReadableStream wraps it.  API consumers should ensure
        // their ReadableStream is configured to dispose of SeekableStream, but
        // currently we only allow for creating TmodFile instances through
        // deserialization.
        readableStream.Dispose();
    }

    private bool Decompress(Span<byte> compressedBytes, Span<byte> uncompressedBytes)
    {
        return decompressor.Decompress(compressedBytes, uncompressedBytes);
    }

#region Deserialization
    /// <summary>
    ///     Reads a <c>.tmod</c> file.
    /// </summary>
    /// <param name="r">
    ///     The reader to read from.
    ///     <br />
    ///     If an older file is being read, the reader may be reinitialized
    ///     under a re-contextualized stream.  For this purpose, you should
    ///     configure the reader to leave the stream open and dispose of it
    ///     yourself afterward.
    /// </param>
    /// <param name="hash">
    ///     The hash span, initialized either to <see cref="HASH_LENGTH"/> to
    ///     read the hash or <c>0</c> to skip reading it.
    /// </param>
    /// <param name="signature">
    ///     The signature span, initialized either to
    ///     <see cref="SIGNATURE_LENGTH"/> to read the signature or <c>0</c> to
    ///     skip reading it.
    /// </param>
    /// <param name="ownsStream">
    ///     The resulting <see cref="TmodFile"/> will hold onto either the file
    ///     stream or a wrapped stream (for legacy files) to facilitate reading
    ///     file data as needed.  If <paramref name="ownsStream"/> is
    ///     <see langword="true"/>, the stream will be cleaned up once the file
    ///     is disposed.
    /// </param>
    /// <returns>The read <c>.tmod</c> file.</returns>
    public static TmodFile Read(
        ref ByteReader r,
        Span<byte>     hash,
        Span<byte>     signature,
        bool           ownsStream = true
    )
    {
        if (hash.Length is not HASH_LENGTH and not 0)
        {
            throw new ArgumentException($"Hash span was not of correct length ({hash.Length}), should be {HASH_LENGTH} or 0", nameof(hash));
        }

        if (signature.Length is not HASH_LENGTH and not 0)
        {
            throw new ArgumentException($"Signature span was not of correct length ({signature.Length}), should be {SIGNATURE_LENGTH} or 0", nameof(signature));
        }

        if (r.U32() != TMOD_MAGIC_HEADER)
        {
            throw new InvalidDataException($"Did not get magic header: {TMOD_MAGIC_HEADER:X8}");
        }

        var tmlVersion = r.NetString();

        if (hash.Length > 0)
        {
            if (r.Span(hash) != HASH_LENGTH)
            {
                throw new InvalidOperationException("Failed to read hash");
            }
        }
        else
        {
            r.Stream.Position += HASH_LENGTH;
        }

        if (signature.Length > 0)
        {
            if (r.Span(signature) != SIGNATURE_LENGTH)
            {
                throw new InvalidOperationException("Failed to read signature");
            }
        }
        else
        {
            r.Stream.Position += SIGNATURE_LENGTH;
        }

        // TODO: Skip the encoded length of the data blob.  We have no use for
        //       it, currently.
        r.Stream.Position += sizeof(uint);

        // Hold onto the old stream so we can position ourselves appropriately
        // in the context of the potentially-unseekable DeflateStream.
        var seekable = r.Stream;
        var isLegacy = System.Version.Parse(tmlVersion) < new_format_milestone;

        if (isLegacy)
        {
            var ds = new DeflateStream(
                r.Stream,
                mode: CompressionMode.Decompress,
                leaveOpen: false
            );
            r.Dispose();
            r = new ByteReader(ds, OwnsStream: false);
        }

        var name    = r.NetString();
        var version = r.NetString();

        var entryCount = r.S32();
        var entries    = new Dictionary<string, Entry>(entryCount);

        if (isLegacy)
        {
            for (var i = 0; i < entryCount; i++)
            {
                var path   = r.NetString();
                var length = r.S32();

                entries.Add(
                    path,
                    new Entry(
                        length,
                        length,
                        seekable.Position
                    )
                );

                seekable.Position += length;
            }
        }
        else
        {
            for (var i = 0; i < entryCount; i++)
            {
                var path             = r.NetString();
                var length           = r.S32();
                var compressedLength = r.S32();

                entries.Add(
                    path,
                    new Entry(
                        length,
                        compressedLength,
                        0
                    )
                );
            }
        }

        foreach (var (path, entry) in entries)
        {
            Debug.Assert(entry.CompressedLength <= entry.UncompressedLength && entry.CompressedLength > 0);

            entries[path] = entry with
            {
                StreamOffset = seekable.Position,
            };

            seekable.Position += entry.CompressedLength;
        }

        return new TmodFile(
            seekable,
            r.Stream,
            ownsStream,
            tmlVersion,
            name,
            version,
            entries
        );
    }
#endregion
}