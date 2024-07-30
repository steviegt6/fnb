using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using U8;

namespace Tomat.FNB.TMOD;

public sealed class TmodFile : ITmodFile
{
    private const char dirty_separator = '\\';
    private const char clean_separator = '/';

    public U8String ModLoaderVersion { get; set; }

    public U8String Name { get; set; }

    public U8String Version { get; set; }

    public IReadOnlyCollection<TmodFileEntry> Entries => entries.Values;

    public TmodFileEntry this[string path] => entries[path];

    // These files are already compressed.
    private static readonly string[] extensions_to_not_compress = [".png", ".mp3", ".ogg"];

    private readonly Dictionary<string, TmodFileEntry> entries;

    public TmodFile(U8String modLoaderVersion, U8String name, U8String version, Dictionary<string, TmodFileEntry> entries)
    {
        ModLoaderVersion = modLoaderVersion;
        Name = name;
        Version = version;
        this.entries = entries;
    }

    public void AddFile(TmodFileData file, uint minimumCompressionSize = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_SIZE, float minimumCompressionTradeoff = TmodConstants.DEFAULT_MINIMUM_COMPRESSION_TRADEOFF)
    {
        file = file with
        {
            Path = SanitizePath(file.Path)
        };

        var size = file.Data.Size;
        if (EligibleForCompression(file, minimumCompressionSize))
        {
            Compress(ref file, size, minimumCompressionTradeoff);
        }

        entries.Add(file.Path, new TmodFileEntry(file.Path, 0, size, size, file.Data));
    }

    public bool RemoveFile(string path)
    {
        throw new NotImplementedException();
    }

    public bool TryWrite(Stream stream)
    {
        throw new NotImplementedException();
    }

    private static string SanitizePath(string path)
    {
        return path.Trim().Replace(dirty_separator, clean_separator);
    }

    private static bool EligibleForCompression(TmodFileData file, uint minCompSize)
    {
        return file.Data.Size >= minCompSize && !extensions_to_not_compress.Contains(Path.GetExtension(file.Path));
    }

    private static void Compress(ref TmodFileData file, int realSize, float tradeoff)
    {
        var data = file.Data;
        var compressed = data.CompressDeflate();

        if (compressed.Size < realSize * tradeoff)
        {
            file = file with
            {
                Data = compressed,
            };
        }
    }
}
