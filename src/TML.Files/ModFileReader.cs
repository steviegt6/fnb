using System.IO;
using System.Text;
using TML.Files.Abstractions;

namespace TML.Files
{
    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IModFileReader"/> implementation.
    /// </summary>
    public class ModFileReader : IModFileReader
    {
        public IModFile Read(Stream stream) {
            using BinaryReader reader = new(stream);
            string header = ConvertToString(reader.ReadBytes(4));
            string modLoaderVersion = reader.ReadString();
            byte[] hash = reader.ReadBytes(20);
            byte[] signature = reader.ReadBytes(256);
            _ = reader.ReadInt32(); // int32 dataLength - no longer used, kept for compatibility
            string name = reader.ReadString();
            string version = reader.ReadString();

            int offset = 0;
            ModFileEntry[] files = new ModFileEntry[reader.ReadInt32()];
            for (int i = 0; i < files.Length; i++) {
                ModFileEntry entry = new()
                {
                    Name = reader.ReadString(),
                    Offset = offset,
                    Length = reader.ReadInt32(),
                    CompressedLength = reader.ReadInt32(),
                };

                files[i] = entry;
                offset += entry.CompressedLength;
            }

            // TODO: Not exactly a "safe" cast. This is safe with legitimate tModLoader mod files, but not necessarily with any made with external tools.
            int fileStartPos = (int) stream.Position;

            foreach (ModFileEntry entry in files) {
                entry.Offset += fileStartPos;
                entry.CachedBytes = reader.ReadBytes(entry.CompressedLength);
            }

            return new ModFile()
            {
                Header = header,
                ModLoaderVersion = modLoaderVersion,
                Hash = hash,
                Signature = signature,
                Name = name,
                Version = version,
                Files = files
            };
        }

        protected static string ConvertToString(byte[] bytes, Encoding? encoding = null) {
            encoding ??= Encoding.ASCII;
            return encoding.GetString(bytes);
        }
    }
}