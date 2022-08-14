using System;
using System.Diagnostics;
using System.IO;

namespace TML.Patcher.Client.Platform
{
    /// <summary>
    ///     Abstract *Nix-focused <see cref="Storage"/> object.
    /// </summary>
    public abstract class UnixStorage : Storage
    {
        /// <inheritdoc cref="Storage.BasePath"/>
        public override string BasePath
        {
            get
            {
                static string GetBasePath()
                {
                    string? xdgPath = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

                    if (string.IsNullOrEmpty(xdgPath))
                        return Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                            ".local",
                            "share"
                        );

                    return xdgPath;
                }

                return Path.Combine(GetBasePath(), "TML-Patcher");
            }
        }

        /// <inheritdoc cref="Storage.PresentDirectoryExternally"/>
        public override void PresentDirectoryExternally(string path) => Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}