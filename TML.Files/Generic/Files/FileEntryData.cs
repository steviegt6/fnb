using System;

namespace TML.Files.Generic.Files
{
    /// <summary>
    ///     Simple container for data of a file entry.
    /// </summary>
    public class FileEntryData
    {
        /// <summary>
        ///     The file's name.
        /// </summary>
        public string fileName;

        /// <summary>
        ///     Data pertaining to the file's length.
        /// </summary>
        public FileLengthData fileLengthData;

        /// <summary>
        ///     Actual file data stored in a byte array.
        /// </summary>
        public byte[] fileData;

        /// <summary>
        ///     Constructs a new <see cref="FileEntryData"/> instance.
        /// </summary>
        public FileEntryData(string name, FileLengthData lengthData, byte[]? data)
        {
            fileName = name;
            fileLengthData = lengthData;
            fileData = data ?? Array.Empty<byte>();
        }
    }
}