using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TML.Files.Abstractions;

namespace TML.Files
{
    public readonly record struct ModFileWriterSettings(
        string MagicHeader,
        string ModLoaderVersion,
        string ModName,
        string ModVersion
    ) : IModFileWriterSettings;

    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IModFileWriter{T}"/> implementation.
    /// </summary>
    public class ModFileWriter : IModFileWriter<ModFileWriterSettings>
    {
        protected class HashSerializer : IDisposable
        {
            public const int HASH_LENGTH = 20;
            public const int MOD_BROWSER_SIGNATURE_LENGTH = 256;
            public const int FILE_DATA_LENGTH = 4;
            
            protected readonly Stream Stream;
            protected readonly BinaryWriter Writer;
            public long HashPos;
            public long DataPos;

            public HashSerializer(Stream stream, BinaryWriter writer) {
                Stream = stream;
                Writer = writer;
            }

            public void MarkHash() {
                HashPos = Stream.Position;
                Stream.Seek(HASH_LENGTH + MOD_BROWSER_SIGNATURE_LENGTH + FILE_DATA_LENGTH, SeekOrigin.Current);
                DataPos = Stream.Position;
            }

            public void Dispose() {
                // Calculate file hash
                Stream.Position = DataPos;
                byte[] hash = SHA1.Create().ComputeHash(Stream);
                
                // Write file hash
                Stream.Position = HashPos;
                Writer.Write(hash);
                
                // Skip writing the signature (ignored in both 1.3 and 1.4, useless with the workshop anyway)..
                Stream.Seek(MOD_BROWSER_SIGNATURE_LENGTH, SeekOrigin.Current);
                
                // Calculate the bytes of the mod's data and write it
                int modBytes = (int) (Stream.Length - DataPos);
                Writer.Write(modBytes);
            }
        }

        public const string MAGIC_HEADER = "TMOD";

        public void Write(IModFile file, Stream stream, ModFileWriterSettings settings) {
            using BinaryWriter writer = new(stream);
            IModFileEntry[] files = file.Files.ToArray();

            writer.Write(Encoding.UTF8.GetBytes(settings.MagicHeader));
            writer.Write(settings.ModLoaderVersion);

            using HashSerializer hashSerializer = new(stream, writer);
            hashSerializer.MarkHash();
                
            writer.Write(settings.ModName);
            writer.Write(settings.ModVersion);
            writer.Write(files.Length);
                
            // Write data for every file entry:
            //  * File path in the .tmod file.
            //  * The uncompressed file length.
            //  * The compressed file length.
            foreach (IModFileEntry entry in files) {
                writer.Write(entry.Name);
                writer.Write(entry.Length);
                writer.Write(entry.CompressedLength);
            }
                
            // Proceed to write file bytes.
            foreach (IModFileEntry entry in files) {
                writer.Write(entry.CachedBytes ?? Array.Empty<byte>());
            }
        }
    }
}