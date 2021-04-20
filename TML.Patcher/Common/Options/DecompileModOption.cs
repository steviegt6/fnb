#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Consolation.Common.Framework.OptionsSystem;
using TML.Patcher.Backend.Decompilation;

namespace TML.Patcher.Common.Options
{
    public class DecompileModOption : ConsoleOption
    {
        public override string Text => "Decompile an extracted mod.";

        public override void Execute()
        {
            string modName = GetModName(Program.Configuration.ExtractPath);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" Decompiling mod: {modName}...");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Stopwatch sw = Stopwatch.StartNew();

            DecompilationRequest request = new(
                Directory.GetFiles(Path.Combine(Program.Configuration.ExtractPath, modName), "*.*")
                    .FirstOrDefault(x => x.EndsWith(".XNA.dll")),
                Path.Combine(Program.Configuration.DecompilePath, modName), 
                Program.Configuration.ReferencesPath,
                modName);

            request.OnError += message => Program.Instance.WriteAndClear(message);

            request.ExecuteRequest();

            sw.Stop();

            Console.WriteLine("Decompilation operation completed.");
            Console.WriteLine($"Completed decompilation operation of {modName} in {sw.Elapsed}.");
            Console.ForegroundColor = ConsoleColor.White;

            Program.Instance.WriteOptionsList(new ConsoleOptions("Return:"));
        }

        private static string GetModName(string pathToSearch)
        {
            while (true)
            {
                Program.Instance.WriteAndClear("Please enter the name of the mod you want to decompile:", ConsoleColor.Yellow);
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    Program.Instance.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (Directory.Exists(Path.Combine(pathToSearch, modName)))
                    return modName;

                Program.Instance.WriteAndClear("Specified mod could not be located!");
            }
        }
    }
}
