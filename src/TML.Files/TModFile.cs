using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TML.Files;

public class TModFile
{
    #region Constants

    public const uint DEFALT_MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1 kilobyte
    public const float DEFAULT_COMPRESSION_TRADEOFF = 0.9f;
    public const string HEADER = "TMOD";

    #endregion

    #region Data

    public string Header { get; set; } = HEADER;

    public virtual string ModLoaderVersion { get; set; } = "";

    public virtual byte[] Hash { get; set; } = Array.Empty<byte>();

    public virtual byte[] Signature { get; set; } = Array.Empty<byte>();

    public virtual string Name { get; set; } = "";

    public virtual string Version { get; set; } = "";

    public virtual IList<TModFileEntry> Files { get; set; } = new List<TModFileEntry>();

    #endregion

    #region File adding

    public virtual void AddFile(TModFileData file, uint minCompSize = DEFALT_MINIMUM_COMPRESSION_SIZE, float compTradeoff = DEFAULT_COMPRESSION_TRADEOFF) {
        file.Path = file.Path.Trim().Replace('\\', '/');

        int size = file.Data.Length;
        if (size > minCompSize && ShouldCompress(file)) Compress(file, size, compTradeoff);
        
        Files.Add(new TModFileEntry
        {
            Path = file.Path,
            Offset = -1,
            Length = size,
            CompressedLength = file.Data.Length,
            Bytes = file.Data
        });
    }

    protected virtual bool ShouldCompress(TModFileData fileData) {
        return new[] {".png", ".mp3", ".ogg"}.Contains(Path.GetExtension(fileData.Path));
    }
    
    protected virtual void Compress(TModFileData file, int realSize, float tradeoff) {
        using MemoryStream ms = new(file.Data.Length);
        using (DeflateStream ds = new(ms, CompressionMode.Compress)) ds.Write(file.Data, 0, file.Data.Length);
        byte[] com = ms.ToArray();
        if (com.Length < realSize * tradeoff) file.Data = com;
    }

    #endregion
}