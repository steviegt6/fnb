using System;
using System.IO;
using TML.Files.Utilities;

namespace TML.Files
{
    public class ModFile : IDisposable
    {
        public ModFileEntry[] FileEntries { get; }

        public string Name { get; }
        
        public Version Version { get; }
        
        public Version ModLoaderVersion { get; }

        public string MagicHeader { get; }

        public byte[] Hash { get; }
        
        public byte[] Signature { get; }

        public FileStream ModStream { get; }

        public ModFile(string path)
        {
            ModStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader reader = new(ModStream);

            MagicHeader = reader.ReadBytes(4).ConvertToString();
            ModLoaderVersion = new Version(reader.ReadString());
            Hash = reader.ReadBytes(20);
            Signature = reader.ReadBytes(256);

            // int dataLength
            _ = reader.ReadInt32();

            // if modLoaderVersion < 0.11 upgrade hhg

            Name = reader.ReadString();
            Version = new Version(reader.ReadString());

            int offset = 0;
            FileEntries = new ModFileEntry[reader.ReadInt32()];

            for (int i = 0; i < FileEntries.Length; i++)
            {
                ModFileEntry entry = new(
                    reader.ReadString(),
                    offset,
                    reader.ReadInt32(),
                    reader.ReadInt32()
                );

                FileEntries[i] = entry;

                offset += entry.CompressedLength;
            }

            int fileStartPos = (int) ModStream.Position;

            foreach (ModFileEntry entry in FileEntries) 
                entry.Offset += fileStartPos;

            foreach (ModFileEntry entry in FileEntries)
                entry.CachedBytes = reader.ReadBytes(entry.CompressedLength);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            ModStream.Dispose();
        }
    }
}