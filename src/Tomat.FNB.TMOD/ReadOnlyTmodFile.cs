using System;
using System.Collections.Generic;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A read-only view into a <c>.tmod</c> file archive.
/// </summary>
public sealed class ReadOnlyTmodFile(ITmodFile tmodFile) : ITmodFile
{
    string ITmodFile.ModLoaderVersion
    {
        get => tmodFile.ModLoaderVersion;
        set => throw new InvalidOperationException("Cannot change the mod loader version of a read-only `.tmod` file!");
    }

    string ITmodFile.Name
    {
        get => tmodFile.Name;
        set => throw new InvalidOperationException("Cannot change the name of a read-only `.tmod` file!");
    }

    string ITmodFile.Version
    {
        get => tmodFile.Version;
        set => throw new InvalidOperationException("Cannot change the version of a read-only `.tmod` file!");
    }

    IReadOnlyCollection<TmodFileEntry> ITmodFile.Entries => tmodFile.Entries;

    TmodFileEntry ITmodFile.this[string path] => tmodFile[path];

    void ITmodFile.AddFile(TmodFileData file, uint minimumCompressionSize, float minimumCompressionTradeoff)
    {
        throw new InvalidOperationException("Cannot add files to a read-only `.tmod` file!");
    }

    bool ITmodFile.RemoveFile(string path)
    {
        throw new InvalidOperationException("Cannot remove files from a read-only `.tmod` file!");
    }
}