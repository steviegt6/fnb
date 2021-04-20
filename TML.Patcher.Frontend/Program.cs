using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Consolation;
using Consolation.Common.Framework.OptionsSystem;
using TML.Patcher.Frontend.Common;

namespace TML.Patcher.Frontend
{
    public static class Program
    {
        public static Version FrontendVersion => new(0, 1, 1, 0);

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

            if (Configuration.ShowIlSpyCmdInstallPrompt)
                InstallILSpyCMD();
            
            Instance.WriteStaticText(false);
            Instance.CheckForUndefinedPath();
            ConsoleAPI.SelectedOptionSet.ListForOption();
        }

        private static void InstallILSpyCMD()
        {
            Console.WriteLine("Do you want to install ilspycmd?");
            Console.WriteLine("<y/n>");
            
            ConsoleKeyInfo pressedKey = Console.ReadKey();
            Console.WriteLine();

            if (pressedKey.Key == ConsoleKey.Y)
            {
                const string dotNetCommand = "dotnet tool install ilspycmd -g";
                
                Console.WriteLine("Attempting to install ilspycmd...");
                
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
                }
                
                process.Start();
                process.WaitForExit();
            }
            
            Configuration.ShowIlSpyCmdInstallPrompt = false;
            ConfigurationFile.Save();
        }
    }
}