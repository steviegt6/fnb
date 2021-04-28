using System;
using System.Diagnostics;
using System.IO;
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
            Patcher window = Consolation.Consolation.GetWindow<Patcher>();
            string modName = Utilities.GetModName(Program.Configuration.ModsPath,
                "Please enter the name of the mod you want to extract:");

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
    }
}