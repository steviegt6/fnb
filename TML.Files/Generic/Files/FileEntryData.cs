using System;

namespace TML.Files.Generic.Files
{
    public struct FileEntryData
    {
        public string fileName;
        public FileLengthData fileLengthData;
        public byte[] fileData;

        public FileEntryData(string fileName, FileLengthData fileLengthData, byte[]? fileData)
        {
            this.fileName = fileName;
            this.fileLengthData = fileLengthData;
            this.fileData = fileData ?? Array.Empty<byte>();
        }
    }
}