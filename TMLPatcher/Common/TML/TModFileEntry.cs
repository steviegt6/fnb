namespace TMLPatcher.Common.TML
{
    // Code taken and modified from Trivaxy's tModViewer
    // https://gyazo.com/a366fcf56c1ed29da86a2ab89a58245c.png
    public readonly struct TModFileEntry
    {
        public readonly struct FileLength
        {
            public readonly int length;
            public readonly int lengthCompressed;

            public FileLength(int length, int lengthCompressed)
            {
                this.length = length;
                this.lengthCompressed = lengthCompressed;
            }
        }

        public readonly string fileName;
        public readonly FileLength fileLength;
        public readonly byte[] fileData;

        public TModFileEntry(string fileName, FileLength fileLength, byte[] fileData)
        {
            this.fileName = fileName;
            this.fileLength = fileLength;
            this.fileData = fileData;
        }
    }
}
