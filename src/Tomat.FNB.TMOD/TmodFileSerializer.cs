using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

using LibDeflate;

namespace Tomat.FNB.TMOD;

public static class TmodFileSerializer
{
    private record struct TmodFileEntry(
        string  Path,
        int     Length,
        int     CompressedLength,
        byte[]? Data
    );

    public static ITmodFile Read(string path)
    {
        using var fs = File.OpenRead(path);
        return Read(fs);
    }

    public static ITmodFile Read(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return Read(ms);
    }

    public static ITmodFile Read(Stream stream)
    {
        var reader = new BinaryReader(stream);

        try
        {
            if (reader.ReadUInt32() != TMOD_HEADER)
            {
                throw new InvalidDataException("Invalid TMOD header!");
            }

            var modLoaderVersion = reader.ReadString();

            stream.Position += HASH_LENGTH
                             + SIGNATURE_LENGTH
                             + sizeof(uint);

            var isLegacy = Version.Parse(modLoaderVersion) < VERSION_0_11_0_0;
            if (isLegacy)
            {
                var ds = new DeflateStream(stream, CompressionMode.Decompress, true);
                reader = new BinaryReader(ds);
            }

            var name    = reader.ReadString();
            var version = reader.ReadString();

            var offset  = 0;
            var entries = new TmodFileEntry[reader.ReadInt32()];

            if (isLegacy)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    var path   = reader.ReadString();
                    var length = reader.ReadInt32();
                    var data   = reader.ReadBytes(length);

                    entries[i] = new TmodFileEntry(path, length, length, data);
                }
            }
            else
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    entries[i] = new TmodFileEntry(
                        reader.ReadString(),
                        reader.ReadInt32(),
                        reader.ReadInt32(),
                        null
                    );

                    offset += entries[i].CompressedLength;
                }

                if (stream.Position >= int.MaxValue)
                {
                    throw new InvalidDataException("TMOD file is too large!");
                }

                for (var i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];

                    entries[i] = entries[i] with
                    {
                        Data = Decompress(reader.ReadBytes(entry.CompressedLength), entry.Length),
                    };
                }
            }

            Debug.Assert(!entries.Any(x => x.Data is null));

            var realEntries = entries.ToDictionary(x => x.Path, x => x.Data!);
            return new TmodFile(modLoaderVersion, name, version, realEntries);
        }
        finally
        {
            reader.Dispose();
        }
    }

    private static byte[] Decompress(byte[] data, int uncompressedLength)
    {
        // In cases where the file isn't actually compressed.  This is possible
        // for smaller files.
        if (data.Length == uncompressedLength)
        {
            return data;
        }

        var array = GC.AllocateUninitializedArray<byte>(uncompressedLength);

        using var ds = new DeflateDecompressor();
        ds.Decompress(data, new Span<byte>(array), out var written);
        {
            Debug.Assert(written == uncompressedLength && array.Length == uncompressedLength);
        }

        return data;
    }
}