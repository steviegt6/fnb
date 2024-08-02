using System.Collections.Generic;
using System.Data;

namespace Tomat.FNB.TMOD;

public readonly struct ReadOnlyTmodFile(ITmodFile tmodFile) : ITmodFile
{
    string ITmodFile.ModLoaderVersion
    {
        get => tmodFile.ModLoaderVersion;
        set => throw new ReadOnlyException("Cannot set ModLoaderVersion of a read-only TmodFile.");
    }

    string ITmodFile.Name
    {
        get => tmodFile.Name;
        set => throw new ReadOnlyException("Cannot set Name of a read-only TmodFile.");
    }

    string ITmodFile.Version
    {
        get => tmodFile.Version;
        set => throw new ReadOnlyException("Cannot set Version of a read-only TmodFile.");
    }

    IReadOnlyDictionary<string, byte[]> ITmodFile.Entries => tmodFile.Entries;

    byte[] ITmodFile.this[string path] => tmodFile[path];

    void ITmodFile.AddFile(string path, byte[] data, long minimumCompressionSize, float minimumCompressionTradeoff)
    {
        throw new ReadOnlyException("Cannot add a file to a read-only TmodFile.");
    }

    bool ITmodFile.RemoveFile(string path)
    {
        throw new ReadOnlyException("Cannot remove a file from a read-only TmodFile.");
    }
}