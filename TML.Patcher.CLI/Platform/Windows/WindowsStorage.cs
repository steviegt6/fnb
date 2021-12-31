using System;
using System.IO;

namespace TML.Patcher.CLI.Platform.Windows
{
    /// <summary>
    ///     The default <see cref="Storage"/> object for Windows computers.
    /// </summary>
    public class WindowsStorage : Storage
    {
        /// <inheritdoc cref="Storage.BasePath"/>
        public override string BasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TML-Patcher");

        /// <inheritdoc cref="Storage.PresentPathExternally"/>
        public override void PresentDirectoryExternally(string path) => Explorer.OpenDirectory(path);
    }
}