using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Consolation;
using Consolation.Framework.OptionsSystem;
using TML.Patcher.Packing;

namespace TML.Patcher.CLI.Common.Options
{
    /// <summary>
    ///     Unpacks the given mod.
    /// </summary>
    public class UnpackModOption : ConsoleOption
    {
        /// <inheritdoc cref="ConsoleOption.Text"/>
        public override string Text => "Unpack a mod.";

        /// <summary>
        ///     Unpacks a mod at the request of the user.
        /// </summary>
        public override void Execute()
        {
            PerformExtraction(Utilities.GetModName(Program.Configuration.ModsPath,
                "Please enter the name of the mod you want to extract:"));

            Program.Patcher.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }

        internal static void PerformExtraction(string pathOrModName)
        {
            Patcher window = Program.Patcher;
            Console.ForegroundColor = ConsoleColor.Yellow;
            string modExtractFolder = Path.Combine(Program.Configuration.ExtractPath, Program.LightweightLoad
                ? Path.GetFileName(pathOrModName)
                : pathOrModName);

            window.WriteLine(Program.LightweightLoad
                ? " Extracting mod..."
                : $" Extracting mod: {pathOrModName}...");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Stopwatch sw = Stopwatch.StartNew();
            ProgressBar bar = ProgressBar.StartNew(Program.Patcher, Program.Configuration.ProgressBarSize);

            new UnpackRequest(Directory.CreateDirectory(modExtractFolder),
                Program.LightweightLoad
                    ? pathOrModName
                    : Path.Combine(Program.Configuration.ModsPath, pathOrModName),
                Program.Configuration.Threads, bar).ExecuteRequest();

            sw.Stop();
            bar.Finish();

            Console.ForegroundColor = ConsoleColor.White;
            window.WriteLine($" Finished extracting mod: {pathOrModName}");
            window.WriteLine($" Extraction time: {sw.Elapsed}");

            if (!Program.LightweightLoad)
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = modExtractFolder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception e) when (e is AccessViolationException or Win32Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to open extracted folder location due to insufficient permissions.");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}