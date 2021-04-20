#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using Consolation.Common.Framework.OptionsSystem;
using TML.Patcher.Backend.Packing;

namespace TML.Patcher.Frontend.Common.Options
{
    public class UnpackModOption : ConsoleOption
    {
        public override string Text => "Unpack a mod.";

        public override void Execute()
        {
            string modName = GetModName(Program.Configuration.ModsPath);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine( $" Extracting mod: {modName}...");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Stopwatch sw = Stopwatch.StartNew();

            UnpackRequest request =
                new(Directory.CreateDirectory(Path.Combine(Program.Configuration.ExtractPath, modName)),
                    Path.Combine(Program.Configuration.ModsPath, modName), Program.Configuration.Threads);

            request.ExecuteRequest();

            sw.Stop();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" Finished extracting mod: {modName}");
            Console.WriteLine($" Extraction time: {sw.Elapsed}");

            Program.Instance.WriteOptionsList(new ConsoleOptions("Return:"));
        }

        private static string GetModName(string pathToSearch)
        {
            while (true)
            {
                Program.Instance.WriteAndClear("Please enter the name of the mod you want to extract:", ConsoleColor.Yellow);
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    Program.Instance.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (File.Exists(Path.Combine(pathToSearch, modName))) 
                    return modName;
                
                Program.Instance.WriteAndClear("Specified mod could not be located!");
            }
        }
    }
}
