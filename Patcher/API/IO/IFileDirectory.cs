using System.Collections.Generic;
using System.IO;
using Patcher.IO;

namespace Patcher.API.IO
{
    /// <summary>
    ///     Wrapper around <see cref="Directory"/>. Automatically creates a directory if it does not exist.
    /// </summary>
    public interface IFileDirectory
    {
        /// <summary>
        ///     The <see cref="DirectoryInfo"/> this <see cref="FileDirectory"/> instance wraps around.
        /// </summary>
        DirectoryInfo Info { get; }

        void RecursiveDelete();

        IEnumerable<FileInfo> EnumerateAllFiles(string filter = "**");

        IEnumerable<DirectoryInfo> EnumerateAllDirectories(string filter = "**");
    }
}