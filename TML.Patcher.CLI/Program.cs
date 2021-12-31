using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CliFx;
using TML.Patcher.CLI.Utilities;

namespace TML.Patcher.CLI
{
    /// <summary>
    ///     Entry-point and main launch handler.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The current program runtime.
        /// </summary>
        public static Runtime? Runtime { get; private set; }

        /// <summary>
        ///     The entrypoint method.
        /// </summary>
        /// <returns></returns>
        public static async Task<int> Main()
        {
            Runtime = new Runtime();

            if (!Runtime.SetupConfig.SetupCompleted)
                RunSetupProcess();

            return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync();
        }

        private static void RunSetupProcess()
        {
            Console.WriteLine(
                "Detected first-time start-up, proceeding with the core set-up process.\n"
            ); // \n is intentional here to create some empty space

            #region References Path

            if (Runtime!.ProgramConfig.ReferencesPath == "undefined")
            {
                if (OperatingSystem.IsWindows())
                {
                    string start = Environment.GetEnvironmentVariable("UserProfile") ?? "";

                    if (Directory.Exists(Path.Combine(start, "OneDrive")))
                        start = Path.Combine(start, "OneDrive");
                    
                    Runtime.ProgramConfig.ReferencesPath = Path.Combine(
                        start,
                        "Documents",
                        "My Games",
                        "Terraria",
                        "ModLoader",
                        "references"
                    );
                }
                else if (OperatingSystem.IsMacOS())
                {
                    Runtime.ProgramConfig.ReferencesPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                        "Library",
                        "Application Support",
                        "Terraria",
                        "ModLoader",
                        "references"
                    );
                }
                else if (OperatingSystem.IsLinux())
                {
                    string? xdgHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

                    if (!string.IsNullOrEmpty(xdgHome))
                    {
                        Runtime.ProgramConfig.ReferencesPath = Path.Combine(
                            xdgHome,
                            "Terraria",
                            "ModLoader",
                            "references"
                        );
                    }
                    else
                    {
                        Runtime.ProgramConfig.ReferencesPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                            ".local",
                            "share",
                            "Terraria",
                            "ModLoader",
                            "references"
                        );
                    }
                }
            }

            DisplayVerify(
                "Checking references path...",
                "Checking references path... (Failed to validate)",
                "Enter a valid references path:",
                "Verified references path.",
                Directory.Exists,
                ref Runtime.ProgramConfig.ReferencesPath
            );

            #endregion

            #region Steam Path
            
            if (Runtime.ProgramConfig.SteamPath == "undefined")
            {
                if (OperatingSystem.IsWindows())
                {
                    Runtime.ProgramConfig.SteamPath = Path.Combine(
                        Environment.Is64BitProcess
                            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                            : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        "Steam",
                        "steamapps",
                        "common",
                        "tModLoader"
                    );
                }
                else if (OperatingSystem.IsMacOS())
                {
                    Runtime.ProgramConfig.SteamPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                        "Library",
                        "Application Support",
                        "Steam",
                        "SteamApps",
                        "common",
                        "tModLoader"
                    );
                }
                else if (OperatingSystem.IsLinux())
                {
                    Runtime.ProgramConfig.SteamPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                        ".steam",
                        "steam",
                        "SteamApps",
                        "common",
                        "tModLoader"
                    );
                }
            }

            DisplayVerify(
                "Checking Steam path... (only Steam is auto-detected, not GoG)",
                "Checking Steam path... (only Steam is auto-detected, not GoG) (Failed to validate)",
                "Enter a valid tModLoader Steam/GoG path:",
                "Verified Steam/GoG path.",
                Directory.Exists,
                ref Runtime.ProgramConfig.SteamPath
            );

            #endregion
        }

        private static void DisplayVerify(
            string originalPrompt,
            string failedPrompt,
            string newPrompt,
            string verified,
            Func<string, bool> verify,
            ref string presetPath
        )
        {
            bool looped = false;

            while (true)
            {
                if (looped)
                    ConsoleUtilities.ClearAboveLines(3);

                WriteProcessText("VERIFYING");
                Console.Write(originalPrompt);

                if (!verify(presetPath))
                {
                    ConsoleUtilities.ClearAboveLines(0);
                    looped = true;
                    WriteProcessText("ERROR", ConsoleColor.DarkRed);
                    Console.WriteLine(failedPrompt);
                    Console.WriteLine(newPrompt);
                    presetPath = Console.ReadLine() ?? "";
                    continue;
                }

                ConsoleUtilities.ClearAboveLines(0);
                WriteProcessText("VERIFIED", ConsoleColor.Green);
                Console.WriteLine(verified);
                break;
            }
        }

        private static void WriteProcessText(
            string text,
            ConsoleColor background = ConsoleColor.White,
            ConsoleColor foreground = ConsoleColor.Black
        )
        {
            Console.ResetColor();
            Console.Write(' ');
            ConsoleUtilities.ColorInvert(background, foreground);
            Console.Write(text);
            Console.ResetColor();
            ConsoleUtilities.WriteMany(' '.ToString(), 10 - text.Length);
            Console.Write(' ');
        }
    }
}