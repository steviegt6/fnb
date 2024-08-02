using System.Collections.Generic;

namespace Tomat.FNB.TMOD;

public interface ITmodFile
{
    string ModLoaderVersion { get; set; }

    string Name { get; set; }

    string Version { get; set; }

    IReadOnlyDictionary<string, byte[]> Entries { get; }

    byte[] this[string path] { get; }

    void AddFile(
        string path,
        byte[] data,
        long   minimumCompressionSize     = DEFAULT_MINIMUM_COMPRESSION_SIZE,
        float  minimumCompressionTradeoff = DEFAULT_MINIMUM_COMPRESSION_TRADEOFF
    );

    bool RemoveFile(string path);
}