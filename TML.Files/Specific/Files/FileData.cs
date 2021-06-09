namespace TML.Files.Specific.Files
{
    /// <summary>
    ///     Struct containing information for a file's hash, and length, as well as the containing file count.
    /// </summary>
    public struct FileDataWithFileCount
    {
        /// <summary>
        ///     The .tmod file's hash.
        /// </summary>
        public string fileHash;

        /// <summary>
        ///     The length of the .tmod file.
        /// </summary>
        public uint fileLength;

        /// <summary>
        ///     The amount of files stored in the .tmod file.
        /// </summary>
        public int fileCount;

        public FileDataWithFileCount(string fileHash, uint fileLength, int fileCount)
        {
            this.fileHash = fileHash;
            this.fileLength = fileLength;
            this.fileCount = fileCount;
        }
    }
}