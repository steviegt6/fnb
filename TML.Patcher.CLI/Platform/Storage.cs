using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TML.Patcher.CLI.Platform
{
    /// <summary>
    ///     Abstracted storage-handling class for proper cross-OS compatibility.
    /// </summary>
    public abstract class Storage
    {
        /// <summary>
        ///     The base directory that this <see cref="Storage"/> instance belongs to, works as the relative path.
        /// </summary>
        public abstract string BasePath { get; }

        /// <summary>
        ///     Creates a directory.
        /// </summary>
        /// <param name="path">The (usually relative) path to create the directory at.</param>
        /// <returns>A <see cref="DirectoryCreationInfo"/> object with the appropriate information.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified path leads to an existing file.</exception>
        public virtual DirectoryCreationInfo CreateDirectory(string path)
        {
            path = GetFullPath(path);

            if (!DirectoryExists(path) && !FileExists(path))
                return new DirectoryCreationInfo(Directory.CreateDirectory(path), true);

            if (!DirectoryExists(path) && FileExists(path))
                throw new InvalidOperationException(
                    "Could not create directory at " + path + " because a file with the same name already exists!"
                );

            return new DirectoryCreationInfo(new DirectoryInfo(path), false);
        }

        /// <summary>
        ///     Retrieves a directory.
        /// </summary>
        /// <param name="path">The (usually relative) path to resolve a directory at.</param>
        /// <param name="create">Whether to create a directory if one does not already exist.</param>
        /// <exception cref="InvalidOperationException">Thrown if the specified path leads to an existing file.</exception>
        public virtual DirectoryInfo GetDirectory(string path, bool create = true)
        {
            path = GetFullPath(path);

            if (!DirectoryExists(path) && !FileExists(path) && create)
                return Directory.CreateDirectory(path);

            if (!DirectoryExists(path) && FileExists(path))
                throw new InvalidOperationException(
                    "Could not resolve directory at " + path + " because a file with the same name already exists!"
                );

            return new DirectoryInfo(path);
        }

        /// <summary>
        ///     Retrieves an enumerable collection of <see cref="DirectoryInfo"/>s.
        /// </summary>
        /// <param name="path">The (usually relative) path to resolve directories at.</param>
        /// <param name="create">Whether to create the directory to search in (for safety).</param>
        public IEnumerable<DirectoryInfo> GetDirectories(string path, bool create = true) => Directory
            .GetDirectories(GetDirectory(path, create).FullName).Select(x => new DirectoryInfo(x));

        /// <summary>
        ///     Retrieves a <see cref="FileInfo"/> at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The (usually relative)> path to search for a file.</param>
        /// <exception cref="InvalidOperationException">If a directory is nothing is resolved at the specified path.</exception>
        public virtual FileInfo GetFile(string path)
        {
            path = GetFullPath(path);

            if (!DirectoryExists(path) && !FileExists(path))
                throw new InvalidOperationException("No file or directory found at " + path + "!");

            if (DirectoryExists(path) && !FileExists(path))
                throw new InvalidOperationException("A directory was found in place of a file at " + path + "!");

            return new FileInfo(path);
        }

        /// <summary>
        ///     Retrieves an enumerable collection of <see cref="FileInfo"/>s.
        /// </summary>
        /// <param name="path">The (usually relative) path to resolve files at.</param>
        /// <param name="create">Whether to create the directory to search in (for safety).</param>
        /// <returns></returns>
        public virtual IEnumerable<FileInfo> GetFiles(string path, bool create = true) =>
            Directory.GetFiles(GetDirectory(path, create).FullName).Select(x => new FileInfo(x));

        /// <summary>
        ///     Checks if a directory exists.
        /// </summary>
        /// <param name="path">The (usually relative) path to check.</param>
        public virtual bool DirectoryExists(string path) => Directory.Exists(GetFullPath(path));

        /// <summary>
        ///     Checks if a file exists.
        /// </summary>
        /// <param name="path">The (usually relative) path to check.</param>
        public virtual bool FileExists(string path) => File.Exists(GetFullPath(path));

        /// <summary>
        ///     Deletes a directory.
        /// </summary>
        /// <param name="path">The (usually relative) path to delete.</param>
        public virtual void DeleteDirectory(string path) => Directory.Delete(GetFullPath(path));

        /// <summary>
        ///     Deletes a file.
        /// </summary>
        /// <param name="path">The (usually relative) path to delete.</param>
        public virtual void DeleteFile(string path) => File.Delete(GetFullPath(path));

        /// <summary>
        ///     Transforms a relative path into a full path.
        /// </summary>
        /// <param name="path">The relative path to append </param>
        /*protected*/ public virtual string GetFullPath(string path) =>
            Path.IsPathRooted(path) ? path : Path.Combine(BasePath, path);

        /// <summary>
        ///     Opens the system's native file browser.
        /// </summary>
        /// <param name="path">The directory to reveal.</param>
        public abstract void PresentDirectoryExternally(string path);
    }
}