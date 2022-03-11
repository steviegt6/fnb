using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using TML.Files.Utilities;

namespace TML.Files
{
    /// <summary>
    ///     .tmod file representation. Contains file and mod data, as well as a list of files.
    /// </summary>
    public class ModFile
    {
        /// <summary>
        ///     The version when tmod files where upgraded to a new format.
        /// </summary>
        protected static readonly Version UpgradeVersion = new(0, 11);
        
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
            BinaryReader reader = Reader;
            
            Header = reader.ReadBytes(4).ConvertToString(); // file header, expected to be TMOD

            string loaderVersionString = reader.ReadString();
            Version loaderVersion = Version.Parse(loaderVersionString);
            string hash = Encoding.ASCII.GetString(reader.ReadBytes(20));

            Signature = reader.ReadBytes(256);

            uint length = reader.ReadUInt32();

            if (loaderVersion < UpgradeVersion) 
            {
                DeflateStream deflateStream = new(reader.BaseStream, CompressionMode.Decompress, true);
                BinaryReader deflateReader = new(deflateStream);
                reader = deflateReader;
            }
            
            string modName = reader.ReadString();
            string modVersionString = reader.ReadString();
            int count = reader.ReadInt32();

            FileDataWithFileCount = new FileDataWithFileCount(hash, length, count);
            FileModData = new ModData(modName, Version.Parse(modVersionString), loaderVersion);

            if (loaderVersion < UpgradeVersion)
                RegisterOldFileEntries(count, reader);
            else
                RegisterFileEntries(count);
            
            if (reader != Reader)
                reader.Close();
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
                byte[] realFileData = Reader.ReadBytes(tempFile.FileLengthData.LengthCompressed);
                Files.Add(new FileEntryData(tempFile.FileName, tempFile.FileLengthData, realFileData));
            }
        }

        /// <summary>
        /// Populates the <see cref="Files"/> list from the old tmod file format.
        /// </summary>
        public virtual void RegisterOldFileEntries(int count, BinaryReader deflateReader)
        {
            for (int i = 0; i < count; i++) {
                string name = deflateReader.ReadString();
                int length = deflateReader.ReadInt32();
                byte[] realFileData = deflateReader.ReadBytes(length);
                
                Files.Add(new FileEntryData(name, new FileLengthData(length, length), realFileData));
            }
        }
    }
}