using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

using Tomat.FNB.Common.BinaryData;

using U8;
using U8.IO;

namespace Tomat.FNB.TMOD;

/// <summary>
///     Serialization utilities for <see cref="ITmodFile"/> implementations.
///     <br />
///     Provides utilities for reading and writing <c>.tmod</c> archives,
///     including handling special tModLoader-defined file formats.
/// </summary>
public static class TmodSerializer
{
    private static readonly Version upgrade_version = new(0, 11, 0, 0);

#region Write
    /// <summary>
    ///     Writes the <c>.tmod</c> archive to a stream.
    /// </summary>
    /// <param name="tmod">The <c>.tmod</c> archive.</param>
    /// <param name="stream">The stream to write ot.</param>
    public static void Write(ITmodFile tmod, Stream stream)
    {
        var writer = new BinaryWriter(stream);

        try
        {
            writer.Write(TmodConstants.TMOD_HEADER);
            writer.Write(tmod.ModLoaderVersion);

            var hashStartPos = stream.Position;
            {
                writer.Write(new byte[TmodConstants.HASH_LENGTH]);
                writer.Write(new byte[TmodConstants.SIGNATURE_LENGTH]);
                writer.Write(0);
            }
            var hashEndPos = stream.Position;

            var isLegacy = Version.Parse(tmod.ModLoaderVersion.ToString()) < upgrade_version;
            if (isLegacy)
            {
                var ms = new MemoryStream();
                var ds = new DeflateStream(ms, CompressionMode.Compress, true);
                writer = new BinaryWriter(ds);
            }

            writer.Write(tmod.Name);
            writer.Write(tmod.Version);
            writer.Write(tmod.Entries.Count);

            if (isLegacy)
            {
                foreach (var entry in tmod.Entries)
                {
                    Debug.Assert(entry.Data is not null, $"{entry.Path} has null data!");

                    writer.Write(entry.Path);
                    writer.Write(entry.Length);
                    entry.Data.Write(writer);
                }
            }
            else
            {
                foreach (var entry in tmod.Entries)
                {
                    writer.Write(entry.Path);
                    writer.Write(entry.CompressedLength);
                    writer.Write(entry.Length);
                }

                foreach (var entry in tmod.Entries)
                {
                    Debug.Assert(entry.Data is not null, $"{entry.Path} has null data!");

                    entry.Data.Write(writer);
                }
            }

            if (isLegacy)
            {
                Debug.Assert(writer.BaseStream is MemoryStream, "BaseStream of writer was somehow not MemoryStream!");

                var compressed = (writer.BaseStream as MemoryStream)!.GetBuffer();
                writer.Dispose();
                writer = new BinaryWriter(stream);
                writer.Write(compressed);
            }

            stream.Position = hashEndPos;
            {
                var hash = SHA1.Create().ComputeHash(stream);
                stream.Position = hashStartPos;
                {
                    writer.Write(hash);
                    writer.Write(new byte[TmodConstants.SIGNATURE_LENGTH]);
                    writer.Write((int)(stream.Length - hashEndPos));
                }
            }
        }
        finally
        {
            writer.Dispose();
        }
    }
#endregion

#region Read
    /// <summary>
    ///     Reads a <c>.tmod</c> archive from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>
    ///     An <see cref="ITmodFile"/> instance contained the read data.
    /// </returns>
    public static ITmodFile Read(Stream stream)
    {
        var reader = new BinaryReader(stream);

        try
        {
            if (reader.ReadUInt32() != TmodConstants.TMOD_HEADER)
            {
                throw new InvalidDataException("Failed to read 'TMOD' header!");
            }

            var modLoaderVersion = (U8String)reader.ReadString();

            // Jump ahead past hashes and signatures.  We could eventually
            // support checking the hash for validation, but it's of low
            // priority.  This never has and never will be a secure method of
            // integrity or validation, I don't know why the tModLoader
            // developers chose to include it in the first place.
            stream.Position += TmodConstants.HASH_LENGTH
                             + TmodConstants.SIGNATURE_LENGTH
                             + sizeof(uint);

            var isLegacy = Version.Parse(modLoaderVersion.ToString()) < upgrade_version;
            if (isLegacy)
            {
                var ds = new DeflateStream(stream, CompressionMode.Decompress, true);
                reader = new BinaryReader(ds);
            }

            var name    = (U8String)reader.ReadString();
            var version = (U8String)reader.ReadString();

            var offset  = 0;
            var entries = new TmodFileEntry[reader.ReadInt32()];

            if (isLegacy)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    var entryPath = reader.ReadString();
                    var entrySize = reader.ReadInt32();
                    var entryData = reader.ReadBytes(entrySize);

                    // The data comes decompressed by the stream reader.
                    var data = DataViewFactory.ByteArray.Create(entryData);

                    entries[i] = new TmodFileEntry(entryPath, offset, entrySize, entrySize, data);
                }
            }
            else
            {
                // The first block of data is the file paths and their lengths.
                for (var i = 0; i < entries.Length; i++)
                {
                    var entryPath             = reader.ReadString();
                    var entryLength           = reader.ReadInt32();
                    var entryCompressedLength = reader.ReadInt32();

                    entries[i] =  new TmodFileEntry(entryPath, offset, entryLength, entryCompressedLength, null);
                    offset     += entryCompressedLength;
                }

                if (stream.Position >= int.MaxValue)
                {
                    throw new InvalidDataException($"Stream position exceeded maximum expected value ({int.MaxValue})!");
                }

                var fileStartPos = (int)stream.Position;

                // The second block of data is the actual compressed data.
                for (var i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];

                    var isCompressed = entry.Length != entry.CompressedLength;
                    var data = isCompressed
                        ? DataViewFactory.ByteArray.Deflate.CreateCompressed(reader.ReadBytes(entry.CompressedLength))
                        : DataViewFactory.ByteArray.Create(reader.ReadBytes(entry.CompressedLength));

                    entries[i] = entries[i] with
                    {
                        Offset = entry.Offset + fileStartPos,
                        Data = data,
                    };
                }
            }

            return new TmodFile(modLoaderVersion, name, version, entries.ToDictionary(x => x.Path, x => x));
        }
        finally
        {
            reader.Dispose();
        }
    }
#endregion
}