using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public Patcher(string filePath)
        {
            if (filePath is {Length: > 0})
            {
                if (File.Exists(filePath) && Path.GetExtension(filePath) == ".tmod")
                    Program.LightweightLoad = true;
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("File not valid for unpacking.");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            if (Program.LightweightLoad)
                InitializeProgramOptions();
        }

        /// <summary>
        ///     Writes text that will always show at the beginning, and should persist after clears.
        /// </summary>
        public override void WriteStaticText(bool withMessage)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGray;

            string[] whatCanThisDoLol =
            {
                "Unpack .tmod files",
                //"Repack .tmod files",
                "Decompile stored assembles in .tmod files",
                //"Patch and recompile assemblies"
            };

            string[] assemblyDisplayBlacklist =
            {
                "System.",
                "netstandard",
                "Microsoft.Win32",
                "Anonymously" // Hosted DynamicMethods Assembly
            };

            WriteLine();
            WriteLine(1, "Welcome to TMLPatcher!");
            WriteLine();

            WriteLine("Running:");
            SpaceCount = 2;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.GetName().Name))
            {
                AssemblyName name = assembly.GetName();

                foreach (string blacklistedName in assemblyDisplayBlacklist)
                    if (name.Name == null || name.Name.Contains(blacklistedName))
                        goto ForceContinue;

                WriteLine($"{name.Name} v{name.Version}");

                ForceContinue: ;
            }

            WriteLine();

            WriteLine(1, "This is a program that allows you to:");
            SpaceCount = 2;

            for (int i = 0; i < whatCanThisDoLol.Length; i++)
                WriteLine($"[{i + 1}] {whatCanThisDoLol[i]}");

            WriteLine(0, Line);
            WriteLine(1, "Loaded with configuration options:");

            SpaceCount = 2;
            WriteLine($"{nameof(Program.Configuration.ModsPath)}: {Program.Configuration.ModsPath}");
            WriteLine($"{nameof(Program.Configuration.ExtractPath)}: {Program.Configuration.ExtractPath}");
            WriteLine($"{nameof(Program.Configuration.DecompilePath)}: {Program.Configuration.DecompilePath}");
            WriteLine($"{nameof(Program.Configuration.ReferencesPath)}: {Program.Configuration.ReferencesPath}");
            WriteLine($"{nameof(Program.Configuration.Threads)}: {Program.Configuration.Threads}");
            WriteLine($"{nameof(Program.Configuration.ProgressBarSize)}: {Program.Configuration.ProgressBarSize}");
            WriteLine($"{nameof(Program.Configuration.ItemsPerPage)}: {Program.Configuration.ItemsPerPage}");

            WriteLine(0, Line);
            SpaceCount = 1;
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine("Please note that if you are trying to decompile a mod,");
            WriteLine("you'll have to add all of the mod's required references to:");
            WriteLine($"\"{Program.Configuration.ReferencesPath}\"");
            WriteLine("(i.e. tModLoader.exe, XNA DLLs, ...)");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            WriteLine(1, Line);
            WriteLine();

            if (!withMessage)
                WriteLine();
        }

        public void CheckForUndefinedPath()
        {
            while (true)
            {
                if (!Program.Configuration.ModsPath.Equals("undefined") &&
                    Directory.Exists(Program.Configuration.ModsPath))
                    return;

                SearchForPathAlternatives();

                Console.ForegroundColor = ConsoleColor.White;
                WriteLine(1, $"{nameof(Program.Configuration.ModsPath)} is undefined or was not found!");
                WriteLine("Please enter the directory of your tModLoader Mods folder:");

                string? modsPath = Console.ReadLine();

                if (Directory.Exists(modsPath))
                {
                    Program.Configuration.ModsPath = modsPath!;
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

        private static void SearchForPathAlternatives()
        {
            if (Directory.Exists(Program.Configuration.ModsPath))
                return;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    if (Directory.Exists(Environment.ExpandEnvironmentVariables(ConfigurationFile.WindowsDefault2)))
                    {
                        Program.Configuration.ModsPath =
                            Environment.ExpandEnvironmentVariables(ConfigurationFile.WindowsDefault2);
                        ConfigurationFile.Save();
                    }

                    break;

                case PlatformID.Unix:
                    if (Directory.Exists(Environment.ExpandEnvironmentVariables(ConfigurationFile.LinuxDefault2)))
                    {
                        Program.Configuration.ModsPath =
                            Environment.ExpandEnvironmentVariables(ConfigurationFile.LinuxDefault2);
                        ConfigurationFile.Save();
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

        public static void InitializeConsoleOptions()
        {
            Program.DefaultOptions = new ConsoleOptions("Pick any option:", 
                Program.Patcher.SelectedOptions,
                new ListModsOption(),
                new ListExtractedModsOption(),
                new ListEnabledModsOption(),
                new UnpackModOption(),
                new DecompileModOption(),
                new RepackModOption(),
                new CreditsAndReleaseNotesOption())
            {
                DisplayReturn = false,
                DisplayGoBack = false
            };

            Program.Patcher.SelectedOptions = Program.DefaultOptions;
        }

        public static void InitializeProgramOptions()
        {
            Program.Configuration =
                ConfigurationFile.Load(Program.ExePath + Path.DirectorySeparatorChar + "configuration.json")!;
        }
    }
}