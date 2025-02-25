using System;
using System.IO;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A partially-deserialized <c>.tmod</c> file, containing header data and
///     known entries, as well as APIs for processing the data labeled by those
///     entries.
/// </summary>
public readonly record struct TmodFile(
    TmodFileHeader Header,
 
)
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

    public static TmodFile Read(BinaryReader r)
    {
        
    }
}