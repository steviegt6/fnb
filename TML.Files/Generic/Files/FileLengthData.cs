namespace TML.Files.Generic.Files
{
    /// <summary>
    ///     Holds the length and compressed length of a file.
    /// </summary>
    public struct FileLengthData
    {
        /// <summary>
        ///     Standard file length.
        /// </summary>
        public int length;

        /// <summary>
        ///     Compressed file length.
        /// </summary>
        public int lengthCompressed;

        /// <summary>
        /// </summary>
        /// <param name="length"></param>
        /// <param name="lengthCompressed"></param>
        public FileLengthData(int length, int lengthCompressed)
        {
            this.length = length;
            this.lengthCompressed = lengthCompressed;
        }
    }
}