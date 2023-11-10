using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TML.Files.Exceptions;

namespace TML.Files;

/// <summary>
///     Handle the serialization and deserialization of .tmod files.
/// </summary>
public static class TModFileSerializer {
    private static readonly Version upgrade_version = new(0, 11, 0, 0);

#region Serialization
    /// <summary>
    ///     The length of the byte array storing the hash of a <see cref="TModFile"/>.
    /// </summary>
    public const int HASH_LENGTH = 20;

    /// <summary>
    ///     The length of the byte array storing the mod browser signature of a <see cref="TModFile"/>.
    /// </summary>
    public const int MOD_BROWSER_SIGNATURE_LENGTH = 256;

    /// <summary>
    ///     The length of the byte array storing the length of the file data of a <see cref="TModFile"/>.
    /// </summary>
    public const int FILE_DATA_LENGTH = 4;

    private class HashSerializer {
        private long hashPosition;
        private long dataPosition;

        public void BeginHash(Stream s, BinaryWriter writer) {
            hashPosition = s.Position;
            s.Seek(HASH_LENGTH + MOD_BROWSER_SIGNATURE_LENGTH + FILE_DATA_LENGTH, SeekOrigin.Current);
            dataPosition = s.Position;
        }

        public void EndHash(Stream s, BinaryWriter w) {
            // Calculate file hash.
            s.Position = dataPosition;
            var hash = SHA1.Create().ComputeHash(s);

            // Write file hash.
            s.Position = hashPosition;
            w.Write(hash);

            // Skip writing the signature (ignored in BOTH 1.3 AND 1.4, useless with the workshop anyway...).
            s.Seek(MOD_BROWSER_SIGNATURE_LENGTH, SeekOrigin.Current);

            // Write file data length.
            w.Write((int) (s.Length - dataPosition));
        }
    }

    /// <summary>
    ///     Serializes a <paramref name="file"/> to the <paramref name="filePath"/>.
    /// </summary>
    /// <param name="file">The <see cref="TModFile"/> to serialize.</param>
    /// <param name="filePath">The path to serialize the <paramref name="file"/> to.</param>
    /// <exception cref="TModFileDirectoryAlreadyExistsException">Thrown if the file path points to an existing directory.</exception>
    public static void Serialize(TModFile file, string filePath) {
        if (Directory.Exists(filePath))
            throw new TModFileDirectoryAlreadyExistsException("Attempted to write .tmod file to directory: " + filePath);

        using var fs = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        Serialize(file, fs);
    }

    /// <summary>
    ///     Serializes a <paramref name="file"/> to the <paramref name="stream"/>.
    /// </summary>
    /// <param name="file">The <see cref="TModFile"/> to serialize.</param>
    /// <param name="stream">The stream to serialize the <paramref name="file"/> to.</param>
    /// <exception cref="TModFileInvalidFileEntryException"></exception>
    public static void Serialize(TModFile file, Stream stream) {
        var entries = file.Entries.ToArray();
        using BinaryWriter w = new(stream);

        w.Write(Encoding.ASCII.GetBytes(file.Header));
        w.Write(file.ModLoaderVersion);

        var hashSerializer = new HashSerializer();
        hashSerializer.BeginHash(stream, w);

        w.Write(file.Name);
        w.Write(file.Version);
        w.Write(entries.Length);

        /* Write data for every file entry:
         *  - Local file path in the .tmod file.
         *  - The uncompressed file length.
         *  - The compressed file length.
         */
        foreach (var entry in entries) {
            w.Write(entry.Path);
            w.Write(entry.Length);
            w.Write(entry.CompressedLength);
        }

        // Process to write actual file contents (data).
        foreach (var entry in entries)
            w.Write(entry.Data ?? throw new TModFileInvalidFileEntryException("Attempted to serialize a TModFileEntry with no data: " + entry.Path));

        hashSerializer.EndHash(stream, w);
    }
#endregion

#region Deserialization
    /// <summary>
    ///     Deserializes the file located at the <paramref name="filePath"/> to a <see cref="TModFile"/> instance.
    /// </summary>
    /// <param name="filePath">The file path to deserialize from.</param>
    /// <returns>A <see cref="TModFile"/> instance.</returns>
    /// <exception cref="TModFileNotFoundException">Thrown if the file at the <paramref name="filePath"/> does not exist.</exception>
    public static TModFile Deserialize(string filePath) {
        if (!File.Exists(filePath))
            throw new TModFileNotFoundException("Cannot deserialize .tmod file as file does not exist:" + filePath);

        using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Deserialize(fs);
    }

    /// <summary>
    ///     Deserializes the <paramref name="data"/> to a <see cref="TModFile"/> instance.
    /// </summary>
    /// <param name="data">The .tmod archive data to deserialize.</param>
    /// <returns>A <see cref="TModFile"/> instance.</returns>
    public static TModFile Deserialize(byte[] data) {
        return Deserialize(new MemoryStream(data));
    }

    /// <summary>
    ///     Deserializes the <paramref name="stream"/> to a <see cref="TModFile"/> instance.
    /// </summary>
    /// <param name="stream">The .tmod archive stream to deserialize.</param>
    /// <returns>A <see cref="TModFile"/> instance.</returns>
    public static TModFile Deserialize(Stream stream) {
        var r = new BinaryReader(stream);

        try {
            var header = ReadHeader(r);
            var modLoaderVersion = r.ReadString();
            var hash = r.ReadBytes(20);
            var signature = r.ReadBytes(256);
            _ = r.ReadInt32();

            var legacy = new Version(modLoaderVersion) < upgrade_version;

            if (legacy) {
                using var ds = new DeflateStream(stream, CompressionMode.Decompress, true);
                r.Dispose();
                r = new BinaryReader(ds);
            }

            var name = r.ReadString();
            var version = r.ReadString();

            var offset = 0;
            var entries = new TModFileEntry[r.ReadInt32()];

            if (legacy) {
                for (var i = 0; i < entries.Length; i++) {
                    var entryName = r.ReadString();
                    var entryLength = r.ReadInt32();
                    var entryData = r.ReadBytes(entryLength);
                    
                    entries[i] = new TModFileEntry {
                        Path = entryName,
                        Offset = offset,
                        Length = entryLength,
                        CompressedLength = entryLength,
                        Data = entryData,
                    };
                }
            }
            else {
                for (var i = 0; i < entries.Length; i++) {
                    offset += (entries[i] = new TModFileEntry {
                        Path = r.ReadString(),
                        Offset = offset,
                        Length = r.ReadInt32(),
                        CompressedLength = r.ReadInt32(),
                        Data = null
                    }).CompressedLength;
                }

                // Not exactly a "safe" cast - it's OK with legitimate tModLoader archives (ones produced with official software), but not OK with archives produced
                // externally by tools such as this one.
                var fileStartPos = (int) stream.Position;

                foreach (var entry in entries) {
                    entry.Offset += fileStartPos;
                    entry.Data = r.ReadBytes(entry.CompressedLength);
                }
            }

            return new TModFile {
                Header = header,
                ModLoaderVersion = modLoaderVersion,
                Hash = hash,
                Signature = signature,
                Name = name,
                Version = version,
                Entries = entries.ToList()
            };
        }
        finally {
            r.Dispose();
        }
    }

    private static string ReadHeader(BinaryReader r) {
        var header = Encoding.ASCII.GetString(r.ReadBytes(4));
        if (header != TModFile.HEADER)
            throw new TModFileInvalidHeaderException($"Expected .tmod file header \"{TModFile.HEADER}\" but got \"{header}\"!");

        return header;
    }
#endregion
}
