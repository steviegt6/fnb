namespace TML.Files.Generic.Files
{
    public readonly struct FileEntryData
    {
        public readonly string fileName;
        public readonly FileLengthData fileLengthData;
        public readonly byte[] fileData;

        public FileEntryData(string fileName, FileLengthData fileLengthData, byte[] fileData)
        {
            this.fileName = fileName;
            this.fileLengthData = fileLengthData;
            this.fileData = fileData;
        }
    }
}