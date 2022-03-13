using System.Collections.Generic;
using System.IO;

namespace Patcher.IO
{
    /// <summary>
    ///     Wrapper around <see cref="Directory"/>. Automatically creates a directory if it does not exist.
    /// </summary>
    public sealed class FileDirectory
    {
        /// <summary>
        ///     The <see cref="DirectoryInfo"/> this <see cref="FileDirectory"/> instance wraps around.
        /// </summary>
        public readonly DirectoryInfo Info;

        /// <summary>
        ///     Constructs a new <see cref="FileDirectory"/> with a path specified by <paramref name="path"/>, also used in <see cref="Info"/>'s instantiation.
        /// </summary>
        /// <param name="paths">The working path to use.</param>
        public FileDirectory(params string[] paths)
        {
            Info = new DirectoryInfo(Path.Combine(paths));
            Info.Create();
        }

        public void RecursiveDelete() => Info.Delete(true);

        public IEnumerable<FileInfo> EnumerateAllFiles(string filter = "**") =>
            Info.EnumerateFiles(filter, SearchOption.AllDirectories);

        public IEnumerable<DirectoryInfo> EnumerateAllDirectories(string filter = "**") =>
            Info.EnumerateDirectories(filter, SearchOption.AllDirectories);

        public override string ToString() => Info.FullName;

        public static implicit operator string(FileDirectory directory) => directory.ToString();
        public static implicit operator FileDirectory(string path) => new(path);
        public static implicit operator FileDirectory(string[] paths) => new(paths);
        public static implicit operator FileDirectory(DirectoryInfo directory) => new(directory.FullName);
    }
}