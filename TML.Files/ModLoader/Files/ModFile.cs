using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TML.Files.Generic.Files;
using TML.Files.Generic.Utilities;

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
        public virtual FileDataWithFileCount FileDataWithFileCount { get; protected set; } = new("", 0u, 0);

        /// <summary>
        ///     Associated .tmod mod data.
        /// </summary>
        public virtual ModData FileModData { get; protected set; } = new("", new Version(), new Version());

        /// <summary>
        ///     A list of all files.
        /// </summary>
        public virtual List<FileEntryData> Files { get; protected set; } = new();

        public string Header { get; protected set; } = "";

        public byte[] Signature { get; protected set; } = Array.Empty<byte>();

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
            Header = Reader.ReadBytes(4).ConvertToString(); // file header, expected to be TMOD

            string loaderVersionString = Reader.ReadString();
            string hash = Encoding.ASCII.GetString(Reader.ReadBytes(20));

            Signature = Reader.ReadBytes(256);

            uint length = Reader.ReadUInt32();
            string modName = Reader.ReadString();
            string modVersionString = Reader.ReadString();
            int count = Reader.ReadInt32();

            FileDataWithFileCount = new FileDataWithFileCount(hash, length, count);
            FileModData = new ModData(modName, Version.Parse(modVersionString), Version.Parse(loaderVersionString));

            RegisterFileEntries(count);
        }

        /// <summary>
        ///     Populates the <see cref="Files"/> list.
        /// </summary>
        public virtual void RegisterFileEntries(int count)
        {
            List<FileEntryData> tempFiles = new();

            for (int i = 0; i < count; i++)
            {
                string name = Reader.ReadString();
                int length = Reader.ReadInt32();
                int lengthCompressed = Reader.ReadInt32();
                tempFiles.Add(new FileEntryData(name, new FileLengthData(length, lengthCompressed), null));
            }

            for (int i = 0; i < count; i++)
            {
                FileEntryData tempFile = tempFiles[i];
                byte[] realFileData = Reader.ReadBytes(tempFile.fileLengthData.lengthCompressed);
                Files.Add(new FileEntryData(tempFile.fileName, tempFile.fileLengthData, realFileData));
            }
        }
    }
}