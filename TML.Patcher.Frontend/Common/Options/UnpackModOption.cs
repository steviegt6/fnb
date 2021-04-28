#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using Consolation;
using Consolation.Common;
using Consolation.Common.Framework.OptionsSystem;
using TML.Patcher.Backend.Packing;

namespace TML.Patcher.Frontend.Common.Options
{
    public class UnpackModOption : ConsoleOption
    {
        public override string Text => "Unpack a mod.";

        public override void Execute()
        {
            Patcher window = ConsoleAPI.GetWindow<Patcher>();
            string modName = GetModName(Program.Configuration.ModsPath);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            window.WriteLine(1, $"Extracting mod: {modName}...");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Stopwatch sw = Stopwatch.StartNew();

            ProgressBar bar = ProgressBar.StartNew(Program.Configuration.ProgressBarSize);

            UnpackRequest request =
                new(Directory.CreateDirectory(Path.Combine(Program.Configuration.ExtractPath, modName)),
                    Path.Combine(Program.Configuration.ModsPath, modName), Program.Configuration.Threads, bar);

            request.ExecuteRequest();

            sw.Stop();

            // Finish reporting the progress
            bar.Finish();

            Console.ForegroundColor = ConsoleColor.White;
            window.WriteLine($"Finished extracting mod: {modName}");
            window.WriteLine($"Extraction time: {sw.Elapsed}");
            window.WriteOptionsList(new ConsoleOptions("Return:"));
        }

        private static string GetModName(string pathToSearch)
        {
            Patcher window = ConsoleAPI.GetWindow<Patcher>();

            while (true)
            {
                window.WriteAndClear("Please enter the name of the mod you want to extract:", ConsoleColor.Yellow);
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    window.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (File.Exists(Path.Combine(pathToSearch, modName))) 
                    return modName;

                window.WriteAndClear("Specified mod could not be located!");
            }
        }
    }
}
