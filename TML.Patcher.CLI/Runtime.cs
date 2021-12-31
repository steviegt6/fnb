using System;
using TML.Patcher.CLI.Configuration;
using TML.Patcher.CLI.Platform;
using TML.Patcher.CLI.Platform.Linux;
using TML.Patcher.CLI.Platform.Mac;
using TML.Patcher.CLI.Platform.Windows;

namespace TML.Patcher.CLI
{
    /// <summary>
    ///     Contains all the base runtime data.
    /// </summary>
    public class Runtime
    {
        /// <summary>
        ///     The <see cref="Storage"/> instance currently in use.
        /// </summary>
        public Storage PlatformStorage { get; }

        /// <summary>
        ///     Indicates if the setup process as previously completed.
        /// </summary>
        public SetupConfig SetupConfig { get; }

        /// <summary>
        ///     Contains program configuration.
        /// </summary>
        public ProgramConfig ProgramConfig { get; }

        internal Runtime()
        {
            if (OperatingSystem.IsWindows())
                PlatformStorage = new WindowsStorage();
            else if (OperatingSystem.IsMacOS())
                PlatformStorage = new MacStorage();
            else if (OperatingSystem.IsLinux())
                PlatformStorage = new LinuxStorage();
            else
                throw new PlatformNotSupportedException("No storage system available for your system.");

            SetupConfig = SetupConfig.DeserializeConfig(PlatformStorage);
            ProgramConfig = ProgramConfig.DeserializeConfig(PlatformStorage);
        }
    }
}