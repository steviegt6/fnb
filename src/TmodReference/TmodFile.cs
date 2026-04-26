using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Tomat.Files.Tmod;

public sealed class TmodFile(
    string path,
    string name,
    Version version,
    Version modLoaderVersion
)
{
    private const uint min_compress_size = 1 << 10; //1KB
    private const float compression_tradeoff = 0.9f;

    private readonly ConcurrentBag<FileEntry> files = [];

    public Version ModLoaderVersion { get; } = modLoaderVersion;

    public string Name { get; } = name;

    public Version Version { get; } = version;

    private static string Sanitize(string path)
    {
        return path.Replace('\\', '/');
    }

    public void AddFile(string fileName, byte[] data)
    {
        fileName = Sanitize(fileName);
        var size = data.Length;

        if (size > min_compress_size && ShouldCompress(fileName))
        {
            using var ms = new MemoryStream(data.Length);
            using (var ds = new DeflateStream(ms, CompressionMode.Compress))
            {
                ds.Write(data, 0, data.Length);
            }

            var compressed = ms.ToArray();
            if (compressed.Length < size * compression_tradeoff)
            {
                data = compressed;
            }
        }

        files.Add(new FileEntry(fileName, -1, size, data.Length, data));
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var fileStream = File.Create(path);
        using var writer = new BinaryWriter(fileStream);

        writer.Write(Encoding.ASCII.GetBytes("TMOD"));
        writer.Write(ModLoaderVersion.ToString());

        var hashPos = (int)fileStream.Position;
        writer.Write(new byte[20 + 256 + 4]);

        var dataPos = (int)fileStream.Position;
        writer.Write(Name);
        writer.Write(Version.ToString());

        writer.Write(files.Count);

        foreach (var f in files)
        {
            Debug.Assert(f.CachedBytes is not null);

            if (f.CompressedLength != f.CachedBytes?.Length)
            {
                throw new Exception($"CompressedLength ({f.CompressedLength}) != cachedBytes.Length ({f.CachedBytes?.Length}): {f.Name}");
            }

            writer.Write(f.Name);
            writer.Write(f.Length);
            writer.Write(f.CompressedLength);
        }

        var offset = (int)fileStream.Position;
        foreach (var f in files)
        {
            Debug.Assert(f.CachedBytes is not null);

            writer.Write(f.CachedBytes ?? throw new IOException("Cannot write null bytes"));

            f.Offset = offset;
            offset += f.CompressedLength;
        }

        fileStream.Position = dataPos;
        var hash = SHA1.Create().ComputeHash(fileStream);

        fileStream.Position = hashPos;
        writer.Write(hash);

        fileStream.Seek(256, SeekOrigin.Current);

        writer.Write((int)(fileStream.Length - dataPos));
    }

    private static bool ShouldCompress(string fileName)
    {
        return !fileName.EndsWith(".png") &&
               !fileName.EndsWith(".mp3") &&
               !fileName.EndsWith(".ogg");
    }
}
