using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TML.Files.Generic.Files;

namespace TML.Files.ModLoader.Files
{
    /// <summary>
    ///     .tmod file representation. Contains file and mod data, as well as a list of files.
    /// </summary>
    public class ModFile
    {
        /// <summary>
        ///     The usable binary reader.
        /// </summary>
        public virtual BinaryReader Reader { get; }

        /// <summary>
        ///     The .tmod's file data.
        /// </summary>
        public virtual FileDataWithFileCount FileDataWithFileCount { get; protected set; }

        /// <summary>
        ///     Associated .tmod mod data.
        /// </summary>
        public virtual ModData FileModData { get; protected set; }

        /// <summary>
        ///     A list of all files.
        /// </summary>
        public virtual List<FileEntryData> Files { get; protected set; } = new();

        /// <summary>
        /// </summary>
        /// <param name="reader"></param>
        public ModFile(BinaryReader reader)
        {
            Reader = reader;
        }

        /// <summary>
        ///     Populates the file data.
        /// </summary>
        public virtual void PopulateFiles()
        {
            _ = Reader.ReadBytes(4); // file header

            string versionString = Reader.ReadString();
            string fileHash = Encoding.ASCII.GetString(Reader.ReadBytes(20));

            _ = Reader.ReadBytes(256); // garbage data(?)

            uint fileLength = Reader.ReadUInt32();
            string modName = Reader.ReadString();
            string modVersionString = Reader.ReadString();
            int fileCount = Reader.ReadInt32();

            FileDataWithFileCount = new FileDataWithFileCount(fileHash, fileLength, fileCount);
            FileModData = new ModData(modName, Version.Parse(modVersionString), Version.Parse(versionString));

            RegisterFileEntries(fileCount);
        }

        /// <summary>
        ///     Populates the <see cref="Files"/> list.
        /// </summary>
        /// <param name="fileCount"></param>
        public virtual void RegisterFileEntries(int fileCount)
        {
            List<FileEntryData> tempFiles = new();

            for (int i = 0; i < fileCount; i++)
            {
                string fileName = Reader.ReadString();
                int fileLength = Reader.ReadInt32();
                int fileLengthCompressed = Reader.ReadInt32();
                tempFiles.Add(new FileEntryData(fileName, new FileLengthData(fileLength, fileLengthCompressed), null));
            }

            for (int i = 0; i < fileCount; i++)
            {
                FileEntryData tempFile = tempFiles[i];
                byte[] realFileData = Reader.ReadBytes(tempFile.fileLengthData.lengthCompressed);
                Files.Add(new FileEntryData(tempFile.fileName, tempFile.fileLengthData, realFileData));
            }
        }
    }
}