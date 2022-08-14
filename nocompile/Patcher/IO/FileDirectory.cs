using System.Collections.Generic;
using System.IO;
using Patcher.API.IO;

namespace Patcher.IO
{
    public sealed class FileDirectory : IFileDirectory
    {
        public DirectoryInfo Info { get; }

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