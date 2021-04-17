namespace TML.Files.Generic
{
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
}