using System.Collections.Generic;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A mutable view into a <c>.tmod</c> file.
/// </summary>
public interface ITmodFile : IReadOnlyTmodFile
{
    /// <summary>
    ///     A hashmap (path -> file data) of all the files within this
    ///     <c>.tmod</c> file.
    /// </summary>
    new IDictionary<string, byte[]> Entries { get; }

    /// <summary>
    ///     Adds a file to the <c>.tmod</c> file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="data">The file data.</param>
    void AddFile(string path, byte[] data);

    /// <summary>
    ///     Removes a file from the <c>.tmod</c> file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>Whether there was a file to remove.</returns>
    bool RemoveFile(string path);
}