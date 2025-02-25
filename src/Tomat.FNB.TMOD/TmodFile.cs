using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using Tomat.FNB.Common.IO;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A partially-deserialized <c>.tmod</c> file, containing header data and
///     known entries, as well as APIs for processing the data labeled by those
///     entries.
/// </summary>
/// <param name="SeekableStream">
///     The stream containing relevant file data which may be directly
///     seeked.
/// </param>
/// <param name="ReadableStream">
///     The stream which should be used to read any file data in case of
///     compression.  It should wrap <paramref name="SeekableStream"/> and it
///     should be configured to dispose of <see cref="SeekableStream"/> when
///     it's disposed.
/// </param>
/// <param name="OwnsStreams">
///     Whether this instance owns the streams and may dispose of them when this
///     instance is disposed.
/// </param>
/// <param name="TmlVersion">
///     The version of tModLoader this <c>.tmod</c> file was created from.
///     <br />
///     Obviously, tools like <b>fnb</b> mean the file may not have been created
///     through tModLoader, so it more realistically indicates the version the
///     mod is intended to be run under.
/// </param>
/// <param name="Name">
///     The uniquely-identifiable, internal name of the mod.  Distinctly not the
///     display name.
/// </param>
/// <param name="Version">
///     The version of the mod, which must be formatted as a
///     <see cref="System.Version.Parse(string)"/>-compatible string.
/// </param>
public readonly record struct TmodFile(
    Stream                            SeekableStream,
    Stream                            ReadableStream,
    bool                              OwnsStreams,
    string                            TmlVersion,
    string                            Name,
    string                            Version,
    Dictionary<string, TmodFileEntry> Entries
) : IDisposable
{
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

    public void Dispose()
    {
        if (!OwnsStreams)
        {
            return;
        }

        // For now, we do not need to call ::Dispose() on SeekableStream since
        // we're assuming ReadableStream wraps it.  API consumers should ensure
        // their ReadableStream is configured to dispose of SeekableStream.
        ReadableStream.Dispose();
    }

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
    ///     The hash span, initialized either to
    ///     <see cref="TmodFileHeader.HASH_LENGTH"/> to read the hash or =
    ///     <c>0</c> to skip.
    /// </param>
    /// <param name="signature">
    ///     The signature span, initialized either to
    ///     <see cref="TmodFileHeader.SIGNATURE_LENGTH"/> to read the signature
    ///     or <c>0</c> to skip.
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
        if (hash.Length is not TmodFileHeader.HASH_LENGTH and not 0)
        {
            throw new ArgumentException($"Hash span was not of correct length ({hash.Length}), should be {TmodFileHeader.HASH_LENGTH} or 0", nameof(hash));
        }

        if (signature.Length is not TmodFileHeader.HASH_LENGTH and not 0)
        {
            throw new ArgumentException($"Signature span was not of correct length ({signature.Length}), should be {TmodFileHeader.SIGNATURE_LENGTH} or 0", nameof(signature));
        }

        if (r.U32() != TmodFileHeader.TMOD_MAGIC_HEADER)
        {
            throw new InvalidDataException($"Did not get magic header: {TmodFileHeader.TMOD_MAGIC_HEADER:X8}");
        }

        var tmlVersion = r.NetString();

        if (hash.Length > 0)
        {
            if (r.Span(hash) != TmodFileHeader.HASH_LENGTH)
            {
                throw new InvalidOperationException("Failed to read hash");
            }
        }
        else
        {
            r.Stream.Position += TmodFileHeader.HASH_LENGTH;
        }

        if (signature.Length > 0)
        {
            if (r.Span(signature) != TmodFileHeader.SIGNATURE_LENGTH)
            {
                throw new InvalidOperationException("Failed to read signature");
            }
        }
        else
        {
            r.Stream.Position += TmodFileHeader.SIGNATURE_LENGTH;
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
            r = new ByteReader(ds, ownsStream: false);
        }

        var name    = r.NetString();
        var version = r.NetString();

        var entryCount = r.S32();
        var entries    = new Dictionary<string, TmodFileEntry>(entryCount);

        if (isLegacy)
        {
            for (var i = 0; i < entryCount; i++)
            {
                var path   = r.NetString();
                var length = r.S32();

                entries.Add(
                    path,
                    new TmodFileEntry(
                        length,
                        length,
                        r.Stream.Position
                    )
                );

                r.Stream.Position += length;
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
                    new TmodFileEntry(
                        length,
                        compressedLength,
                        0
                    )
                );
            }
        }
    }
}