using System.Collections.Generic;
using System.Data;

namespace Tomat.FNB.TMOD;

#region ITmodFile
public interface ITmodFile
{
    string ModLoaderVersion { get; set; }

    string Name { get; set; }

    string Version { get; set; }

    IReadOnlyDictionary<string, byte[]> Entries { get; }

    byte[] this[string path] { get; }

    void AddFile(string path, byte[] data);

    bool RemoveFile(string path);
}
#endregion

#region ReadOnlyTmodFile
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

    void ITmodFile.AddFile(string path, byte[] data)
    {
        throw new ReadOnlyException("Cannot add a file to a read-only TmodFile.");
    }

    bool ITmodFile.RemoveFile(string path)
    {
        throw new ReadOnlyException("Cannot remove a file from a read-only TmodFile.");
    }
}
#endregion

public sealed class TmodFile(
    string                     modLoaderVersion,
    string                     name,
    string                     version,
    Dictionary<string, byte[]> entries
) : ITmodFile
{
    public string ModLoaderVersion { get; set; } = modLoaderVersion;

    public string Name { get; set; } = name;

    public string Version { get; set; } = version;

    public IReadOnlyDictionary<string, byte[]> Entries => entries;

    public byte[] this[string path] => Entries[path];

    public void AddFile(string path, byte[] data)
    {
        path = SanitizePath(path);

        entries[path] = data;
    }

    public bool RemoveFile(string path)
    {
        path = SanitizePath(path);

        return entries.Remove(path);
    }

    private static string SanitizePath(string path)
    {
        const char dirty_separator = '\\';
        const char clean_separator = '/';

        return path.Trim().Replace(dirty_separator, clean_separator);
    }
}