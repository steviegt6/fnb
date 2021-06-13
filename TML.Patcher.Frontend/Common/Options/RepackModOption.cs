using System;
using System.Diagnostics;
using System.IO;
using Consolation.Common.Framework.OptionsSystem;
using TML.Files.Specific.Files;
using TML.Patcher.Backend.Packing;

namespace TML.Patcher.Frontend.Common.Options
{
    public class RepackModOption : ConsoleOption
    {
        public override string Text => "Repack a mod.";
        public override void Execute()
        {
            PerformRepack(Utilities.GetModName(Program.Configuration.ExtractPath,
                "Please enter the name of the mod you want to repack:", true));
            
            Program.Patcher.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }

        private static void PerformRepack(string pathOrModName)
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

            new RepackRequest(Directory.CreateDirectory(modFolder), targetFilePath,
                    new ModData("PotatoKnishes", new Version(0, 11, 8, 4), new Version(1, 2, 3, 4)), Program.Configuration.Threads)
                .ExecuteRequest();
            
            sw.Stop();
            
            Console.ForegroundColor = ConsoleColor.White;
            window.WriteLine($"Finished repacking mod: {pathOrModName}");
            window.WriteLine($"Repack time: {sw.Elapsed}");
        }
    }
}