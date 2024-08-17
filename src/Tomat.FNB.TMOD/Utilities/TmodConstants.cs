global using static Tomat.FNB.TMOD.Utilities.TmodConstants;

using System;

namespace Tomat.FNB.TMOD.Utilities;

/// <summary>
///     Well-known constants pertaining to the <c>.tmod</c> file format.
/// </summary>
internal static class TmodConstants
{
    /// <summary>
    ///     The default minimum size for a file to be compressed.
    /// </summary>
    public const long DEFAULT_MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1 KiB

    /// <summary>
    ///     The default minimum tradeoff for a file to be compressed.
    /// </summary>
    public const float DEFAULT_MINIMUM_COMPRESSION_TRADEOFF = 0.9f;

    /// <summary>
    ///     The header of a <c>.tmod</c> file.
    /// </summary>
    public const uint TMOD_HEADER = 0x444F4D54; // "TMOD"

    /// <summary>
    ///     The length of the hash in a <c>.tmod</c> file.
    /// </summary>
    public const int HASH_LENGTH = 20;

    /// <summary>
    ///     The length of the signature in a <c>.tmod</c> file.
    /// </summary>
    public const int SIGNATURE_LENGTH = 256;

    /// <summary>
    ///     Version <c>0.11.0.0</c>, which indicates the first change in the
    ///     <c>.tmod</c> file format.
    /// </summary>
    public static readonly Version VERSION_0_11_0_0 = new(0, 11, 0, 0);
}