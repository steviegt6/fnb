using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

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
}
