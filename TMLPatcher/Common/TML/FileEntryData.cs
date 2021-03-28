namespace TMLPatcher.Common.TML
{
    // Code taken and modified from Trivaxy's tModViewer
    // https://gyazo.com/a366fcf56c1ed29da86a2ab89a58245c.png
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
