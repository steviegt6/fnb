using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Consolation;
using Consolation.Framework.OptionsSystem;
using Microsoft.Win32;
using TML.Patcher.CLI.Common;
using TML.Patcher.CLI.Common.Options;

namespace TML.Patcher.CLI
{
    /// <summary>
    ///     Entry-point and main launch handler.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     Executable path.
        /// </summary>
        public static string ExePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        /// <summary>
        ///     Configuration instance read from a config file.
        /// </summary>
        public static ConfigurationFile Configuration { get; set; } = null!;

        /// <summary>
        ///     Fallback options used in the <see cref="Patcher"/> instance.
        /// </summary>
        public static ConsoleOptions DefaultOptions { get; set; } = null!;

        /// <summary>
        ///     Main <see cref="ConsoleWindow"/> instance.
        /// </summary>
        public static Patcher Patcher { get; private set; } = null!;

        /// <summary>
        ///     Whether or not this is a quick, light-weight load with the aim of extracting a single file.
        /// </summary>
        public static bool LightweightLoad { get; set; }

        /// <summary>
        ///     Entry-point.
        /// </summary>
        /// <param name="args">Drag-and-drop file argument array because DragonFruit is funny like that.</param>
        /// <param name="path">File path for extracting a single file quickly.</param>
        /// <param name="skipILSpyCMDPrompt">Skips the ILSpyCMD installation prompt.</param>
        /// <param name="skipRegistryPrompt">Skips the Windows registry prompt.</param>
        public static void Main(string[] args, string path = "", bool skipILSpyCMDPrompt = false, bool skipRegistryPrompt = false)
        {
            Console.Title = "TMLPatcher - by convicted tomatophile";
            Thread.CurrentThread.Name = "Main";

            if (args is not {Length: 0} && string.IsNullOrEmpty(path))
                path = args[0];

            Patcher = new Patcher(path);

            PreLoadAssemblies();
            Patcher.InitializeConsoleOptions();
            Patcher.InitializeProgramOptions();

            if (Configuration.ShowILSpyCMDInstallPrompt && !skipILSpyCMDPrompt)
                InstallILSpyCMD();

            if (Configuration.ShowRegistryAdditionPrompt && !skipRegistryPrompt)
                AddRegistryContext();

            ConfigurationFile.Save();

            if (LightweightLoad)
            {
                UnpackModOption.PerformExtraction(path);
                return;
            }

            Patcher.WriteStaticText(false);
            Patcher.CheckForUndefinedPath();
            Patcher.SelectedOptions.ListForOption(Patcher);
        }

        private static void InstallILSpyCMD()
        {
            Configuration.ShowILSpyCMDInstallPrompt = false;

            Patcher window = Patcher;

            window.WriteLine("Do you want to install ilspycmd?");
            window.WriteLine("<y/n>");

            ConsoleKeyInfo pressedKey = Console.ReadKey();
            window.WriteLine();

            if (pressedKey.Key != ConsoleKey.Y)
                return;

            const string dotNetCommand = "dotnet tool install ilspycmd -g";

            window.WriteLine("Attempting to install ilspycmd...");

            Process process = new();

            if (OperatingSystem.IsWindows())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + dotNetCommand,
                    UseShellExecute = false
                };
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = "-c \" " + dotNetCommand + " \"",
                    UseShellExecute = false
                };
            }
            else
                throw new PlatformNotSupportedException("Unsupported platform for installing ilspycmd.");

            process.Start();
            process.WaitForExit();
        }

        private static void AddRegistryContext()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Patcher window = Patcher;

            window.WriteLine(
                "Do you want to add TML.Patcher.Frontend to your file context menu? Please ensure that this program is located in a location it will not move from.");
            window.WriteLine("<y/n> (or 'p' to skip for now and preserve the prompt for later)");

            ConsoleKeyInfo pressedKey = Console.ReadKey();
            window.WriteLine();

            if (pressedKey.Key == ConsoleKey.P)
                return;

            if (pressedKey.Key != ConsoleKey.Y)
            {
                Configuration.ShowRegistryAdditionPrompt = false;
                return;
            }

            try
            {
                RegistryKey open = Registry.ClassesRoot.CreateSubKey("*\\shell\\Open with TML.Patcher");
                RegistryKey command = open.CreateSubKey("command");
                open.SetValue(null, "Open with TML.Patcher");
                open.SetValue("icon", Path.Combine(ExePath, "TML.Patcher.CLI.exe"));
                command.SetValue(null, $"{Path.Combine(ExePath, "TML.Patcher.CLI.exe")} \"%1\"");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Insufficient permissions to modify the registry." +
                                  "\nPlease re-launch with admin permissions to add to your context menu, otherwise press any key to continue.");
                Console.ReadKey();
            }

            Configuration.ShowRegistryAdditionPrompt = false;

        }

        internal static void PreLoadAssemblies()
        {
            if (LightweightLoad) return;

            List<Assembly> loaded = AppDomain.CurrentDomain.GetAssemblies().ToList();

            Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(x => !loaded.Select(y => y.Location).Contains(x, StringComparer.InvariantCultureIgnoreCase))
                .ToList().ForEach(z => loaded.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(z))));
        }
    }
}