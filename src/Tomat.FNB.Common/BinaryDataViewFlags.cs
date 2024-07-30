using System;

namespace Tomat.FNB.Common;

/// <summary>
///     Informative flags describing an <see cref="IBinaryDataView"/>.
/// </summary>
[Flags]
public enum BinaryDataViewFlags : byte
{
    /// <summary>
    ///     No flags.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Whether this data view has been compressed with the DEFLATE
    ///     algorithm.
    /// </summary>
    CompressedDeflate = 1 << 0,

    /// <summary>
    ///     Whether this data view has been compressed.
    /// </summary>
    Compressed = CompressedDeflate
}
