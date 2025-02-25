using System;

namespace Tomat.FNB.TMOD.Converters;

/// <summary>
///     An arbitrary file converter for a <c>.tmod</c> file.
/// </summary>
public interface IFileConverter
{
    /// <summary>
    ///     Whether this converter should be applied to a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="data">The file data.</param>
    /// <returns>Whether this converter is applicable.</returns>
    bool ShouldConvert(string path, Span<byte> data);

    /// <summary>
    ///     Converts a file and invokes <see cref="onCovert"/> which the
    ///     modified data.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="data">The file data.</param>
    /// <param name="onCovert">
    ///     The delegate to invoke upon conversion.  Contains the modified path
    ///     and file data.
    /// </param>
    /// <returns>
    ///     Whether this file was successfully converter.
    /// </returns>
    /// <remarks>
    ///     This variant exists to reduce allocations by allowing an API
    ///     consumer to keep the created file on the stack if applicable.
    /// </remarks>
    /// <see cref="Convert(string,System.Span{byte})"/>
    bool Convert(
        string                     path,
        Span<byte>                 data,
        Action<string, Span<byte>> onCovert
    );

    /// <summary>
    ///     Converts a file and returns the result instead of delegating it to
    ///     an input function.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="data">The file data.</param>
    /// <returns>
    ///     The modified file path and data, or <see langword="null"/> if
    ///     conversion failed.
    /// </returns>
    /// <see cref="Convert(string,System.Span{byte},System.Action{string,System.Span{byte}})"/>
    (string path, byte[] data)? Convert(
        string     path,
        Span<byte> data
    );
}