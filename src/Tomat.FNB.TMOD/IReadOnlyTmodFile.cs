using System.Collections.Generic;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A read-only view into a <c>.tmod</c> file.
/// </summary>
public interface IReadOnlyTmodFile
{
    /// <summary>
    ///     The version of the mod loader that created this <c>.tmod</c> file.
    /// </summary>
    string ModLoaderVersion { get; }

    /// <summary>
    ///     The internal name of the mod within this <c>.tmod</c> file.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The version of the mod within this <c>.tmod</c> file.
    /// </summary>
    string Version { get; }

    /// <summary>
    ///     A hashmap (path -> file data) of all the files within this
    ///     <c>.tmod</c> file.
    /// </summary>
    IReadOnlyDictionary<string, byte[]> Entries { get; }

    /// <summary>
    ///     Retrieves the file data for the given path.
    /// </summary>
    /// <param name="path">
    ///     The path to the file within the <c>.tmod</c> file.
    /// </param>
    byte[] this[string path] { get; }
}