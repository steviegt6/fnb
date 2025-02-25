using System;

namespace Tomat.FNB.Common.IO.Compression;

/// <summary>
///     Decompresses bytes.
/// </summary>
public interface IDecompressor : IDisposable
{
    /// <summary>
    ///     Decompresses <paramref cref="compressedBytes"/> to
    ///     <paramref name="uncompressedBytes"/>.
    /// </summary>
    /// <param name="compressedBytes">
    ///     The span to read from, containing the compressed bytes.
    /// </param>
    /// <param name="uncompressedBytes">
    ///     The span to write to, containing the uncompressed bytes.
    /// </param>
    /// <returns></returns>
    bool Decompress(Span<byte> compressedBytes, Span<byte> uncompressedBytes);
}