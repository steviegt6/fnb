using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TML.Files.Generic.Files;

namespace TML.Files.Specific.Files
{
    public class TModFile
    {
        public List<FileEntryData> files = new();
        public ModData fileModData;
        public FileData fileData;
        public string fileSig;

        public TModFile(BinaryReader reader) => PopulateDefault(reader);

        private void PopulateDefault(BinaryReader reader) => PopulateFiles(reader);

        public virtual void PopulateFiles(BinaryReader reader)
        {
            _ = reader.ReadBytes(4); // file header

            string versionString = reader.ReadString();
            string fileHash = Encoding.ASCII.GetString(reader.ReadBytes(20));

            _ = reader.ReadBytes(256); // garbage data(?)

            uint fileLength = reader.ReadUInt32();
            string modName = reader.ReadString();
            string modVersionString = reader.ReadString();
            int fileCount = reader.ReadInt32();

            fileData = new FileData(fileHash, fileLength, fileCount);
            fileModData = new ModData(modName, Version.Parse(modVersionString), Version.Parse(versionString));

            RegisterFileEntries(reader, fileCount);
        }

        public virtual void RegisterFileEntries(BinaryReader reader, int fileCount)
        {
            List<FileEntryData> tempFiles = new();

            for (int i = 0; i < fileCount; i++)
            {
                string fileName = reader.ReadString();
                int fileLength = reader.ReadInt32();
                int fileLengthCompressed = reader.ReadInt32();
                tempFiles.Add(new FileEntryData(fileName, new FileLengthData(fileLength, fileLengthCompressed), null));
            }

            for (int i = 0; i < fileCount; i++)
            {
                FileEntryData tempFile = tempFiles[i];
                byte[] realFileData = reader.ReadBytes(tempFile.fileLengthData.lengthCompressed);
                files.Add(new FileEntryData(tempFile.fileName, tempFile.fileLengthData, realFileData));
            }
        }
    }
}
