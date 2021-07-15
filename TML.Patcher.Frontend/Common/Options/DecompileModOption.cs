using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Consolation.Common.Framework.OptionsSystem;
using TML.Patcher.Decompilation;

namespace TML.Patcher.CLI.Common.Options
{
    public class DecompileModOption : ConsoleOption
    {
        public override string Text => "Decompile an extracted mod.";

        public override void Execute()
        {
            Patcher window = Program.Patcher;
            string modName = Utilities.GetModName(Program.Configuration.ExtractPath,
                "Please enter the name of the mod you want to decompile:", true);

            Console.ForegroundColor = ConsoleColor.Yellow;
            window.WriteLine(1, $"Decompiling mod: {modName}...");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Stopwatch sw = Stopwatch.StartNew();

            DecompilationRequest request = new(
                Directory.GetFiles(Path.Combine(Program.Configuration.ExtractPath, modName), "*.*")
                    .FirstOrDefault(x => x.EndsWith(".XNA.dll")),
                Path.Combine(Program.Configuration.DecompilePath, modName),
                Program.Configuration.ReferencesPath,
                modName);

            request.OnError += message => window.WriteAndClear(message);

            request.ExecuteRequest();

            sw.Stop();

            window.WriteLine("Decompilation operation completed.");
            window.WriteLine($"Completed decompilation operation of {modName} in {sw.Elapsed}.");
            Console.ForegroundColor = ConsoleColor.White;

            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}