using System;

namespace TML.Files
{
    /// <summary>
    ///     Simple container for data of a file entry.
    /// </summary>
    public class FileEntryData
    {
        /// <summary>
        ///     The file's name.
        /// </summary>
        public string FileName;

        /// <summary>
        ///     Data pertaining to the file's length.
        /// </summary>
        public FileLengthData FileLengthData;

        /// <summary>
        ///     Actual file data stored in a byte array.
        /// </summary>
        public byte[] FileData;

        /// <summary>
        ///     Constructs a new <see cref="FileEntryData"/> instance.
        /// </summary>
        public FileEntryData(string name, FileLengthData lengthData, byte[]? data)
        {
            FileName = name;
            FileLengthData = lengthData;
            FileData = data ?? Array.Empty<byte>();
        }
    }
}