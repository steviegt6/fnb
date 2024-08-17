using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

namespace Tomat.FNB.TMOD;

/// <summary>
///     A serializable, read-only <c>.tmod</c> file.
/// </summary>
public readonly struct SerializableTmodFile : ISerializableTmodFile
{
    public string ModLoaderVersion { get; }

    public string Name { get; }

    public string Version { get; }

    public IReadOnlyDictionary<string, ISerializableTmodFile.FileEntry> Entries { get; }

    IReadOnlyDictionary<string, byte[]> IReadOnlyTmodFile.Entries => rawDataEntries;

    public ISerializableTmodFile.FileEntry this[string path] => Entries[path];

    byte[] IReadOnlyTmodFile.this[string path] => rawDataEntries[path];

    private readonly Dictionary<string, byte[]> rawDataEntries;

    internal SerializableTmodFile(
        string                                              modLoaderVersion,
        string                                              name,
        string                                              version,
        Dictionary<string, ISerializableTmodFile.FileEntry> entries
    )
    {
        Debug.Assert(entries.Values.All(x => x.Data is not null), "All entries must have data.");

        ModLoaderVersion = modLoaderVersion;
        Name             = name;
        Version          = version;
        Entries          = entries;
        rawDataEntries   = entries.ToDictionary(x => x.Key, x => x.Value.Data!);
    }

    /// <summary>
    ///     Writes this <c>.tmod</c> file to the given stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void Write(Stream stream)
    {
        var writer = new BinaryWriter(stream);

        try
        {
            writer.Write(TMOD_HEADER);
            writer.Write(ModLoaderVersion);

            var hashStartPos = stream.Position;
            {
                const int offset = HASH_LENGTH
                                 + SIGNATURE_LENGTH
                                 + sizeof(uint);
                writer.Write(new byte[offset]);
            }
            var hashEndPos = stream.Position;

            var isLegacy = System.Version.Parse(ModLoaderVersion) < VERSION_0_11_0_0;
            if (isLegacy)
            {
                // Older versions of `.tmod` files compressed the entire file
                // table instead of individual entries when it deemed it
                // necessary.
                var ms = new MemoryStream();
                var ds = new DeflateStream(ms, CompressionMode.Compress, true);
                writer = new BinaryWriter(ds);
            }

            writer.Write(Name);
            writer.Write(Version);
            writer.Write(Entries.Count);

            if (isLegacy)
            {
                foreach (var (path, entry) in Entries)
                {
                    Debug.Assert(entry.CompressedLength == 0);
                    Debug.Assert(entry.Data is not null);
                    Debug.Assert(entry.Length == entry.Data.Length);

                    writer.Write(path);
                    writer.Write(entry.Length);
                    writer.Write(entry.Data);
                }
            }
            else
            {
                foreach (var (path, entry) in Entries)
                {
                    Debug.Assert(entry.CompressedLength <= entry.Length);
                    Debug.Assert(entry.Data is not null);
                    Debug.Assert(entry.Length == entry.Data.Length || entry.CompressedLength == entry.Data.Length);

                    writer.Write(path);
                    writer.Write(entry.Length);
                    writer.Write(entry.CompressedLength);
                }

                foreach (var (_, entry) in Entries)
                {
                    Debug.Assert(entry.Data is not null);

                    writer.Write(entry.Data);
                }
            }

            if (isLegacy)
            {
                Debug.Assert(writer.BaseStream is MemoryStream);

                // TODO(perf): Prefer GetBuffer?
                var compressedData = ((MemoryStream)writer.BaseStream).ToArray();
                writer.Dispose();
                writer = new BinaryWriter(stream);
                writer.Write(compressedData);
            }

            stream.Position = hashEndPos;
            {
                var hash = SHA1.Create().ComputeHash(stream);

                stream.Position = hashStartPos;
                {
                    writer.Write(hash);
                    writer.Write(new byte[SIGNATURE_LENGTH]);
                    writer.Write((int)(stream.Length - hashEndPos));
                }
            }
        }
        finally
        {
            writer.Dispose();
        }
    }

    public static SerializableTmodFile FromStream(Stream stream)
    {
        var reader = new BinaryReader(stream);

        try
        {
            if (reader.ReadUInt32() != TMOD_HEADER)
            {
                throw new InvalidDataException("Invalid TMOD header!");
            }

            var modLoaderVersion = reader.ReadString();
            stream.Position += HASH_LENGTH + SIGNATURE_LENGTH + sizeof(uint);

            var isLegacy = System.Version.Parse(modLoaderVersion) < VERSION_0_11_0_0;
            if (isLegacy)
            {
                var ds = new DeflateStream(stream, CompressionMode.Decompress, true);
                reader = new BinaryReader(ds);
            }

            var name       = reader.ReadString();
            var version    = reader.ReadString();
            var entryCount = reader.ReadInt32();

            var entries = new Dictionary<string, ISerializableTmodFile.FileEntry>(entryCount);

            if (isLegacy)
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    var path   = reader.ReadString();
                    var length = reader.ReadInt32();
                    var data   = reader.ReadBytes(length);

                    entries.Add(
                        path,
                        new ISerializableTmodFile.FileEntry
                        {
                            Length           = length,
                            CompressedLength = length,
                            Data             = data,
                        }
                    );
                }
            }
            else
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    var path             = reader.ReadString();
                    var length           = reader.ReadInt32();
                    var compressedLength = reader.ReadInt32();

                    entries.Add(
                        path,
                        new ISerializableTmodFile.FileEntry
                        {
                            Length           = length,
                            CompressedLength = compressedLength,
                            Data             = null,
                        }
                    );
                }

                foreach (var (path, entry) in entries)
                {
                    Debug.Assert(entry.CompressedLength <= entry.Length && entry.CompressedLength != 0);

                    var data = reader.ReadBytes(entry.CompressedLength);
                    {
                        Debug.Assert(data.Length == entry.CompressedLength);
                    }

                    entries[path] = entry with
                    {
                        Data = data,
                    };
                }
            }

            return new SerializableTmodFile(modLoaderVersion, name, version, entries);
        }
        finally
        {
            reader.Dispose();
        }
    }
}