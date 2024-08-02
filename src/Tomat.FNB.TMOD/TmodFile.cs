using System.Collections.Generic;

namespace Tomat.FNB.TMOD;

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