namespace TML.Files
{
    /// <summary>
    ///     Holds the length and compressed length of a file.
    /// </summary>
    public struct FileLengthData
    {
        /// <summary>
        ///     Standard file length.
        /// </summary>
        public int Length;

        /// <summary>
        ///     Compressed file length.
        /// </summary>
        public int LengthCompressed;

        /// <summary>
        /// </summary>
        /// <param name="length"></param>
        /// <param name="lengthCompressed"></param>
        public FileLengthData(int length, int lengthCompressed)
        {
            Length = length;
            LengthCompressed = lengthCompressed;
        }
    }
}