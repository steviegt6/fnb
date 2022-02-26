namespace TML.Files.Generic.Files
{
    /// <summary>
    ///     Holds the length and compressed length of a file.
    /// </summary>
    public readonly struct FileLengthData
    {
        /// <summary>
        ///     Standard file length.
        /// </summary>
        public readonly int Length;

        /// <summary>
        ///     Compressed file length.
        /// </summary>
        public readonly int LengthCompressed;

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