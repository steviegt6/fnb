using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        public static string EXEPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static ConfigurationFile Configuration { get; set; }

        public static ConsoleOptions DefaultOptions { get; set; }

        public static Patcher Instance { get; private set; }

        public static void Main(string[] args)
        {
            PreLoadAssemblies();

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

                    case PlatformID.Xbox:
                    case PlatformID.Other:
                        Console.WriteLine("Current platform is not supported.");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                process.Start();
                process.WaitForExit();
            }
            
            Configuration.ShowIlSpyCmdInstallPrompt = false;
            ConfigurationFile.Save();
        }

        [SuppressMessage("Style", "IDE0059", 
            Justification = "Assembly pre-loading.")]
        private static void PreLoadAssemblies()
        {
            // ReSharper disable once NotAccessedVariable
            // ReSharper disable once JoinDeclarationAndInitializer
            Type
            discard = typeof(global:: Consolation                               .ConsoleAPI);           // Consolation
            discard = typeof(global::  Newtonsoft.Json                          .JsonConvert);          // Newtonsoft.JSON
            discard = typeof(global::         TML.Files  .Generic .Data         .ImagePixelColor);      // TML.Files
            discard = typeof(global::         TML.Patcher.Backend .Decompilation.DecompilationRequest); // TML.Patcher.Backend
            discard = typeof(global::         TML.Patcher.Frontend              .Program);              // TML.Patcher.Frontend
        }
    }
}