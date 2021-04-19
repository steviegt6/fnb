#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Consolation.Common.Framework.OptionsSystem;
using Newtonsoft.Json;

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

            string? fileName = Directory.GetFiles(Path.Combine(Program.Configuration.ExtractPath, modName), "*.*").FirstOrDefault(x => x.EndsWith(".XNA.dll"));

            if (fileName == null)
            {
                Program.Instance.WriteAndClear($"Unable to locate file: {Path.Combine(Program.Configuration.ExtractPath, modName)}.XNA.dll");
                return;
            }

            Directory.CreateDirectory(Path.Combine(Program.Configuration.DecompilePath, modName));
            Directory.CreateDirectory(Program.Configuration.ReferencesPath);
            string commandArgs = $"\"{fileName}\" --referencepath \"{Program.Configuration.ReferencesPath}\" --outputdir \"{Path.Combine(Program.Configuration.DecompilePath, modName)}\" --project --languageversion \"CSharp7_3\"";

            Console.WriteLine($"Starting CMD process with arguments: {commandArgs}");

            ProcessStartInfo youShouldWork = new("ilspycmd.exe")
            {
                UseShellExecute = true,
                Arguments = commandArgs
            };

            Stopwatch sw = new();
            sw.Start();
            Process? process = Process.Start(youShouldWork);
            process?.WaitForExit();
            sw.Start();

            Console.WriteLine("Decompilation completed.");
            Console.WriteLine($"Decompiled {modName} in {sw.Elapsed}!");
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
