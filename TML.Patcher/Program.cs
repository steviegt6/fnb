using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Consolation;
using TML.Patcher.Common;
using TML.Patcher.Common.Framework;
using TML.Patcher.Common.Options;

namespace TML.Patcher
{
    public static class Program
    {
        public static string EXEPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public const string Line = "-----------------------------------------------------------------";

        public static ConfigurationFile Configuration { get; set; }

        public static ConsoleOptions DefaultOptions { get; set; }

        public static ConsoleOptions SelectedOptions { get; set; }

        public static void Main(string[] args)
        {
            Console.Title = "TMLPatcher - by convicted tomatophile";
            Thread.CurrentThread.Name = "Main";

            ConsoleAPI.Initialize();
            ConsoleAPI.ParseParameters(args);

            InitializeConsoleOptions();
            InitializeProgramOptions();

            WriteStaticText(false);
            CheckForUndefinedPath();
            SelectedOptions.ListForOption();
        }

        /// <summary>
        /// Writes text that will always show at the beginning, and should persist after clears.
        /// </summary>
        public static void WriteStaticText(bool withMessage)
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
                "Trivaxy - Unpack code"
            };

            Console.WriteLine();
            Console.WriteLine(" Welcome to TMLPatcher!");
            Console.WriteLine(" This is a program that allows you to:");

            for (int i = 0; i < whatCanThisDoLol.Length; i++)
                Console.WriteLine($"  [{i + 1}] {whatCanThisDoLol[i]}");

            Console.WriteLine();

            Console.WriteLine(" Credits:");
            foreach (string contributor in contributors)
                Console.WriteLine($"  {contributor}");

            Console.WriteLine(Line);
            Console.WriteLine(" Loaded with configuration options:");
            Console.WriteLine($"  {nameof(Configuration.ModsPath)}: {Configuration.ModsPath}");
            Console.WriteLine($"  {nameof(Configuration.ExtractPath)}: {Configuration.ExtractPath}");
            Console.WriteLine($"  {nameof(Configuration.DecompilePath)}: {Configuration.DecompilePath}");
            Console.WriteLine($"  {nameof(Configuration.ReferencesPath)}: {Configuration.ReferencesPath}");

            Console.WriteLine(Line);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" Please note that if you are trying to decompile a mod,");
            Console.WriteLine(" you'll have to add all of the mod's required references");
            Console.WriteLine(" to /References/! (i.e. tModLoader.exe, XNA DLLs, ...)");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine(Line);
            Console.WriteLine();

            if (!withMessage)
                Console.WriteLine();
        }

        public static void CheckForUndefinedPath()
        {
            while (true)
            {
                if (!Configuration.ModsPath.Equals("undefined"))
                    return;

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" {nameof(Configuration.ModsPath)} is undefined!");
                Console.WriteLine(" Please enter the directory of your tModLoader Mods folder:");
                
                string modsPath = Console.ReadLine();

                if (Directory.Exists(modsPath))
                {
                    Configuration.ModsPath = modsPath;
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
            DefaultOptions = new ConsoleOptions("Pick any option:", new ListModsOption(), new ListExtractedModsOption(), new ListEnabledModsOption(), new UnpackModOption())
            {
                DisplayReturn = false,
                DisplayGoBack = false
            };

            SelectedOptions = DefaultOptions;
        }

        public static void InitializeProgramOptions() => Configuration = ConfigurationFile.Load(EXEPath + Path.DirectorySeparatorChar + "configuration.json");

        public static void Clear(bool withMessage)
        {
            Console.Clear();
            WriteStaticText(withMessage);
        }

        public static void WriteAndClear(string message, ConsoleColor color = ConsoleColor.Red)
        {
            Clear(true);
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteOptionsList(ConsoleOptions options)
        {
            SelectedOptions = options;
            SelectedOptions.ListForOption();
        }
    }
}
