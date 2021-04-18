using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Consolation;
using Consolation.Common.Framework.OptionsSystem;
using TML.Patcher.Common;

namespace TML.Patcher
{
    public static class Program
    {
        public static string EXEPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static ConfigurationFile Configuration { get; set; }

        public static ConsoleOptions DefaultOptions { get; set; }

        public static Patcher Instance { get; private set; }

        public static void Main(string[] args)
        {
            Console.Title = "TMLPatcher - by convicted tomatophile";
            Thread.CurrentThread.Name = "Main";

            Instance = new Patcher();
            ConsoleAPI.Window = Instance;

            ConsoleAPI.Initialize();
            ConsoleAPI.ParseParameters(args);

            Patcher.InitializeConsoleOptions();
            Patcher.InitializeProgramOptions();
            InstallILSpyCMD();

            Instance.WriteStaticText(false);
            Instance.CheckForUndefinedPath();
            ConsoleAPI.SelectedOptionSet.ListForOption();
        }

        private static void InstallILSpyCMD()
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    // TODO: verify this works on other platforms (doubt it)
                    FileName = "cmd.exe",
                    Arguments = "/C dotnet tool install ilspycmd -g",
                    UseShellExecute = false
                }
            };
            Console.WriteLine("Attempting to install ilspycmd...");
            process.Start();
            process.WaitForExit();
        }
    }
}