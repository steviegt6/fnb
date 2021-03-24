using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TMLPatcher.Common.TML
{
    // Code taken and modified from Trivaxy's tModViewer
    // https://gyazo.com/a366fcf56c1ed29da86a2ab89a58245c.png
    public class TModFile
    {
        public readonly struct ModData
        {
            public readonly string modName;
            public readonly Version modVersion;
            public readonly Version modLoaderVersion;

            public ModData(string modName, Version modVersion, Version modLoaderVersion)
            {
                this.modName = modName;
                this.modVersion = modVersion;
                this.modLoaderVersion = modLoaderVersion;
            }
        }

        public readonly struct FileData
        {
            public readonly string fileHash;
            public readonly uint fileLength;
            public readonly int fileCount;

            public FileData(string fileHash, uint fileLength, int fileCount)
            {
                this.fileHash = fileHash;
                this.fileLength = fileLength;
                this.fileCount = fileCount;
            }
        }

        public List<TModFileEntry> files = new();
        public ModData fileModData;
        public FileData fileData;
        public string fileSig;

        public TModFile(BinaryReader reader)
        {
            reader.ReadBytes(4); // file header

            Version tMLVersion = Version.Parse(reader.ReadString());
            string fileHash = Encoding.ASCII.GetString(reader.ReadBytes(20));
            reader.ReadBytes(256); // garbage data(?)
            uint fileLength = reader.ReadUInt32();
            string modName = reader.ReadString();
            Version modVersion = Version.Parse(reader.ReadString());
            int fileCount = reader.ReadInt32();

            fileData = new FileData(fileHash, fileLength, fileCount);
            fileModData = new ModData(modName, modVersion, tMLVersion);

            for (int i = 0; i < fileCount; i++)
                files.Add(new TModFileEntry(reader.ReadString(), new TModFileEntry.FileLength(reader.ReadInt32(), reader.ReadInt32()), null));
            for (int i = 0; i < fileCount; i++)
            {
                TModFileEntry file = files[i];
                files[i] = new TModFileEntry(file.fileName, file.fileLength, reader.ReadBytes(file.fileLength.lengthCompressed));
            }
        }
    }
}
