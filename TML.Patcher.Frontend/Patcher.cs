using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Consolation;
using Consolation.Common;
using Consolation.Common.Framework.OptionsSystem;
using TML.Patcher.Frontend.Common;
using TML.Patcher.Frontend.Common.Options;

namespace TML.Patcher.Frontend
{
    public sealed class Patcher : ConsoleWindow
    {
        public const string Line = "-----------------------------------------------------------------";

        public override ConsoleOptions DefaultOptions => Program.DefaultOptions;

        /// <summary>
        /// Writes text that will always show at the beginning, and should persist after clears.
        /// </summary>
        public override void WriteStaticText(bool withMessage)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGray;

            string[] whatCanThisDoLol =
            {
                "Unpack .tmod files",
                "Repack .tmod files",
                "Decompile stored assembles in .tmod files",
                "Patch and recompile assemblies"
            };

            string[] contributors =
            {
                "convicted tomatophile (Stevie) - Main developer",
                "Trivaxy - Original mod unpacking code",
                "Chik3r - Improved multithreading/task code",
                "Archanyhm - Help with Linux and Mac compatibility"
            };

            string[] releaseNotes =
            {
                "Release Notes - v0.1.2.0",
                " * Added release notes.",
                " * Added configurable progress bar.",
                " * Official splitting of the frontend and backend.",
                " * Internal code clean-up."
            };

            string[] assemblyDisplayBlacklist =
            {
                "System.",
                "netstandard",
                "Microsoft.Win32",
                "Anonymously" // Hosted DynamicMethods Assembly
            };

            ConsoleAPI.Window.WriteLine();
            ConsoleAPI.Window.WriteLine(1, "Welcome to TMLPatcher!");
            ConsoleAPI.Window.WriteLine();

            ConsoleAPI.Window.WriteLine("Running:");
            ConsoleAPI.Window.SpaceCount = 2;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.GetName().Name))
            {
                AssemblyName name = assembly.GetName();

                foreach (string blacklistedName in assemblyDisplayBlacklist)
                    if (name.Name == null || name.Name.Contains(blacklistedName))
                        goto ForceContinue;

                ConsoleAPI.Window.WriteLine($"{name.Name} v{name.Version}");

                ForceContinue: ;
            }

            ConsoleAPI.Window.WriteLine();

            ConsoleAPI.Window.WriteLine(1, "This is a program that allows you to:");
            ConsoleAPI.Window.SpaceCount = 2;

            for (int i = 0; i < whatCanThisDoLol.Length; i++)
                ConsoleAPI.Window.WriteLine($"[{i + 1}] {whatCanThisDoLol[i]}");

            ConsoleAPI.Window.WriteLine();

            ConsoleAPI.Window.WriteLine(1, "Credits:");
            ConsoleAPI.Window.SpaceCount = 2;
            foreach (string contributor in contributors)
                ConsoleAPI.Window.WriteLine($"{contributor}");

            ConsoleAPI.Window.WriteLine(0, Line);
            ConsoleAPI.Window.WriteLine(1, "Loaded with configuration options:");

            ConsoleAPI.Window.SpaceCount = 2;
            ConsoleAPI.Window.WriteLine($"{nameof(Program.Configuration.ModsPath)}: {Program.Configuration.ModsPath}");
            ConsoleAPI.Window.WriteLine($"{nameof(Program.Configuration.ExtractPath)}: {Program.Configuration.ExtractPath}");
            ConsoleAPI.Window.WriteLine($"{nameof(Program.Configuration.DecompilePath)}: {Program.Configuration.DecompilePath}");
            ConsoleAPI.Window.WriteLine($"{nameof(Program.Configuration.ReferencesPath)}: {Program.Configuration.ReferencesPath}");
            ConsoleAPI.Window.WriteLine($"{nameof(Program.Configuration.Threads)}: {Program.Configuration.Threads}");
            ConsoleAPI.Window.WriteLine($"{nameof(Program.Configuration.ProgressBarSize)}: {Program.Configuration.ProgressBarSize}");

            ConsoleAPI.Window.WriteLine(0, Line);
            ConsoleAPI.Window.SpaceCount = 1;
            Console.ForegroundColor = ConsoleColor.Yellow;
            ConsoleAPI.Window.WriteLine("Please note that if you are trying to decompile a mod,");
            ConsoleAPI.Window.WriteLine("you'll have to add all of the mod's required references to:");
            ConsoleAPI.Window.WriteLine($"\"{Program.Configuration.ReferencesPath}\"");
            ConsoleAPI.Window.WriteLine("(i.e. tModLoader.exe, XNA DLLs, ...)");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            ConsoleAPI.Window.WriteLine(0, Line);
            foreach (string note in releaseNotes)
                ConsoleAPI.Window.WriteLine(1, $"{note}");

            ConsoleAPI.Window.WriteLine(1, Line);
            ConsoleAPI.Window.WriteLine();

            if (!withMessage)
                ConsoleAPI.Window.WriteLine();
        }

        public void CheckForUndefinedPath()
        {
            while (true)
            {
                if (!Program.Configuration.ModsPath.Equals("undefined") && Directory.Exists(Program.Configuration.ModsPath))
                    return;

                if (!Directory.Exists(Program.Configuration.ModsPath))
                {
                    switch (Environment.OSVersion.Platform)
                    {
                        case PlatformID.Win32S:
                        case PlatformID.Win32Windows:
                        case PlatformID.Win32NT:
                        case PlatformID.WinCE:
                            if (Directory.Exists(Environment.ExpandEnvironmentVariables(ConfigurationFile.WindowsDefault2)))
                            {
                                Program.Configuration.ModsPath = Environment.ExpandEnvironmentVariables(ConfigurationFile.WindowsDefault2);
                                ConfigurationFile.Save();
                                return;
                            }
                            break;

                        case PlatformID.Unix:
                            if (Directory.Exists(Environment.ExpandEnvironmentVariables(ConfigurationFile.LinuxDefault2)))
                            {
                                Program.Configuration.ModsPath = Environment.ExpandEnvironmentVariables(ConfigurationFile.LinuxDefault2);
                                ConfigurationFile.Save();
                                return;
                            }
                            break;

                        case PlatformID.Xbox:
                        case PlatformID.MacOSX:
                        case PlatformID.Other:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
                ConsoleAPI.Window.WriteLine(1, $"{nameof(Program.Configuration.ModsPath)} is undefined or was not found!");
                ConsoleAPI.Window.WriteLine("Please enter the directory of your tModLoader Mods folder:");
                
                string modsPath = Console.ReadLine();

                if (Directory.Exists(modsPath))
                {
                    Program.Configuration.ModsPath = modsPath;
                    ConfigurationFile.Save();
                    WriteAndClear("New specified path accepted!", ConsoleColor.Green);
                }
                else
                {
                    WriteAndClear("Whoops! The specified path does not exist! Please enter a valid directory.");
                    continue;
                }

                break;
            }
        }

        public static void InitializeConsoleOptions()
        {
            Program.DefaultOptions = new ConsoleOptions("Pick any option:", new ListModsOption(), new ListExtractedModsOption(), new ListEnabledModsOption(), new UnpackModOption(), new DecompileModOption())
            {
                DisplayReturn = false,
                DisplayGoBack = false
            };

            ConsoleAPI.SelectedOptionSet = Program.DefaultOptions;
        }

        public static void InitializeProgramOptions() => Program.Configuration = ConfigurationFile.Load(Program.ExePath + Path.DirectorySeparatorChar + "configuration.json");
    }
}
