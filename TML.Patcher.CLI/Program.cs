using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Consolation.Framework.OptionsSystem;
using Microsoft.Win32;
using TML.Patcher.CLI.Common;
using TML.Patcher.CLI.Common.Options;

namespace TML.Patcher.CLI
{
    public static class Program
    {
        public static string ExePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        public static ConfigurationFile Configuration { get; set; } = null!;

        public static ConsoleOptions DefaultOptions { get; set; } = null!;

        public static Patcher Patcher { get; private set; } = null!;

        public static bool LightweightLoad { get; set; }

        public static void Main(string filePath = "")
        {
            Console.Title = "TMLPatcher - by convicted tomatophile";
            Thread.CurrentThread.Name = "Main";

            Patcher = new Patcher(filePath);

            PreLoadAssemblies();
            Patcher.InitializeConsoleOptions();
            Patcher.InitializeProgramOptions();

            if (Configuration.ShowIlSpyCmdInstallPrompt)
                InstallILSpyCMD();

            if (Configuration.ShowRegistryAdditionPrompt)
                AddRegistryContext();

            ConfigurationFile.Save();

            if (LightweightLoad)
            {
                UnpackModOption.PerformExtraction(filePath);
                return;
            }

            Patcher.WriteStaticText(false);
            Patcher.CheckForUndefinedPath();
            Patcher.SelectedOptions.ListForOption(Patcher);
        }

        private static void InstallILSpyCMD()
        {
            Configuration.ShowIlSpyCmdInstallPrompt = false;

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

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C " + dotNetCommand,
                        UseShellExecute = false
                    };
                    break;

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "bash",
                        Arguments = "-c \" " + dotNetCommand + " \"",
                        UseShellExecute = false
                    };
                    break;

                case PlatformID.Xbox:
                case PlatformID.Other:
                    window.WriteLine("Current platform is not supported.");
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            process.Start();
            process.WaitForExit();
        }

        private static void AddRegistryContext()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Patcher window = Patcher;

            window.WriteLine("Do you want to add TML.Patcher.Frontend to your file context menu? Please ensure that this program is located in a location it will not move from.");
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
                open.SetValue("icon", Path.Combine(ExePath, "TML.Patcher.Frontend.exe"));
                command.SetValue(null, $"{Path.Combine(ExePath, "TML.Patcher.Frontend.exe")} \"%1\"");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Insufficient permissions to modify the registry. Please re-launch with admin permissions to add to your context menu, otherwise press any key to continue.");
                Console.ReadKey();
            }

            Configuration.ShowRegistryAdditionPrompt = false;

        }

        internal static void PreLoadAssemblies()
        {
            if (LightweightLoad)
                return;

            List<Assembly> loaded = AppDomain.CurrentDomain.GetAssemblies().ToList();

            Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(x => !loaded
                    .Select(y => y.Location)
                    .Contains(x, StringComparer.InvariantCultureIgnoreCase))
                .ToList()
                .ForEach(z => loaded
                    .Add(AppDomain.CurrentDomain
                        .Load(AssemblyName.GetAssemblyName(z))));
        }
    }
}