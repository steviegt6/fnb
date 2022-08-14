using System.IO;

namespace TML.Patcher.Client.Platform
{
    /// <summary>
    ///     Holds basic information about the creation of a directory in a <see cref="Storage"/> object.
    /// </summary>
    public readonly struct DirectoryCreationInfo
    {
        /// <summary>
        ///     The <see cref="DirectoryInfo"/> that the path points to.
        /// </summary>
        public readonly DirectoryInfo DirectoryInfo;
        
        /// <summary>
        ///     Whether the directory was created by the <see cref="Storage"/> object.
        /// </summary>
        public readonly bool Created;

        public DirectoryCreationInfo(DirectoryInfo directoryInfo, bool created)
        {
            DirectoryInfo = directoryInfo;
            Created = created;
        }
    }
}