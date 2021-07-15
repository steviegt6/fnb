namespace TML.Files.ModLoader.Files
{
    /// <summary>
    ///     Struct containing information for a file's hash, and length, as well as the containing file count.
    /// </summary>
    public class FileDataWithFileCount
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

        public FileDataWithFileCount(string hash, uint length, int count)
        {
            fileHash = hash;
            fileLength = length;
            fileCount = count;
        }
    }
}