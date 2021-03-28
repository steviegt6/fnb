using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMLPatcher.Common.FileSystem;

namespace TMLPatcher.Common.TML
{
    // Code taken and modified from Trivaxy's tModViewer
    // https://gyazo.com/a366fcf56c1ed29da86a2ab89a58245c.png
    public class TModFile
    {
        public List<FileEntryData> files = new();
        public ModData fileModData;
        public FileData fileData;
        public string fileSig;

        public TModFile(BinaryReader reader) => PopulateFile(reader);

        public void PopulateFile(BinaryReader reader)
        {
            reader.ReadBytes(4); // file header

            Version loaderVersion = Version.Parse(reader.ReadString());
            string fileHash = Encoding.ASCII.GetString(reader.ReadBytes(20));
            reader.ReadBytes(256); // garbage data(?)
            uint fileLength = reader.ReadUInt32();
            string modName = reader.ReadString();
            Version modVersion = Version.Parse(reader.ReadString());
            int fileCount = reader.ReadInt32();

            fileData = new FileData(fileHash, fileLength, fileCount);
            fileModData = new ModData(modName, modVersion, loaderVersion);

            PopulateFiles(reader, fileCount);
        }

        public void PopulateFiles(BinaryReader reader, int fileCount)
        {
            for (int i = 0; i < fileCount; i++)
                files.Add(new FileEntryData(reader.ReadString(), new FileLengthData(reader.ReadInt32(), reader.ReadInt32()), null));
            for (int i = 0; i < fileCount; i++)
            {
                FileEntryData file = files[i];
                files[i] = new FileEntryData(file.fileName, file.fileLengthData, reader.ReadBytes(file.fileLengthData.lengthCompressed));
            }
        }
    }
}
