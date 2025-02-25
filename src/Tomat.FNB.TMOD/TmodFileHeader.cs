using System;
using System.IO;

using Tomat.FNB.Common.IO;

namespace Tomat.FNB.TMOD;

/// <summary>
///     The header data of a <c>.tmod</c> file.
/// </summary>
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
public readonly record struct TmodFileHeader(
    string TmlVersion,
    string Name,
    string Version
)
{
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

    /// <summary>
    ///     Reads a <c>.tmod</c> file header.
    /// </summary>
    /// <param name="r">The reader to read from.</param>
    /// <param name="hash">
    ///     The hash span, initialized either to <see cref="HASH_LENGTH"/> to
    ///     read the hash or <c>0</c> to skip.
    /// </param>
    /// <param name="signature">
    ///     The signature span, initialized either to
    ///     <see cref="SIGNATURE_LENGTH"/> to read the signature or <c>0</c> to
    ///     skip.
    /// </param>
    /// <returns>The read <c>.tmod</c> file header.</returns>
    public static TmodFileHeader Read(
        ByteReader r,
        Span<byte> hash,
        Span<byte> signature
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
    }
}