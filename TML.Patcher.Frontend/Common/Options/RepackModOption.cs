using System;
using System.Diagnostics;
using System.IO;
using Consolation;
using Consolation.Framework.OptionsSystem;
using TML.Files.Specific.Files;
using TML.Patcher.Packing;

namespace TML.Patcher.CLI.Common.Options
{
    public class RepackModOption : ConsoleOption
    {
        public override string Text => "Repack a mod.";
        public override void Execute()
        {
            PerformRepack(Utilities.GetModName(Program.Configuration.ExtractPath,
                "Please enter the name of the mod you want to repack:", true), RequestModData());
            
            Program.Patcher.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }

        private static ModData RequestModData()
        {
            Patcher window = Program.Patcher;

            // Ask for the internal name of the mod
            string modInternalName = RequestSimpleValue("Enter an internal name for the mod", window);

            // Ask for the mod's version
            Version modVersion = Version.Parse(RequestSimpleValue("Enter the version of the mod", window,
                val => Version.TryParse(val, out _)));
            
            // Ask for the mod loader's version
            Version modLoaderVersion = Version.Parse(RequestSimpleValue("Enter the version of the mod loader this mod was compiled for", window,
                val => Version.TryParse(val, out _)));

            return new ModData(modInternalName, modVersion, modLoaderVersion);
        }

        private static string RequestSimpleValue(string query, ConsoleWindow window, Func<string, bool> isValid = null)
        {
            while (true)
            {
                window.WriteAndClear(query);
                string value = Console.ReadLine()!;
                
                if (!string.IsNullOrWhiteSpace(value) && (isValid == null || isValid?.Invoke(value) == true))
                    return value;
            }
        }

        private static void PerformRepack(string pathOrModName, ModData modData)
        {
            Patcher window = Program.Patcher;
            Console.ForegroundColor = ConsoleColor.Yellow;
            
            string targetFilePath = Path.Combine(Program.Configuration.RepackPath, pathOrModName);
            Directory.CreateDirectory(Program.Configuration.RepackPath);
            string modFolder = Path.Combine(Program.Configuration.ExtractPath, pathOrModName);

            window.WriteLine(1, Program.LightweightLoad 
                ? "Repacking mod..." 
                : $"Repacking mod: {pathOrModName}...");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            
            Stopwatch sw = Stopwatch.StartNew();

            new RepackRequest(Directory.CreateDirectory(modFolder), targetFilePath, modData, Program.Configuration.Threads)
                .ExecuteRequest();
            
            sw.Stop();
            
            Console.ForegroundColor = ConsoleColor.White;
            window.WriteLine($"Finished repacking mod: {pathOrModName}");
            window.WriteLine($"Repack time: {sw.Elapsed}");
        }
    }
}