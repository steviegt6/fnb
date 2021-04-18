using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TML.Patcher.Common.Framework;

namespace TML.Patcher.Common.Options
{
    public class ListEnabledModsOption : ConsoleOption
    {
        public override string Text => "List all enabled tML mods.";

        public override void Execute()
        {
            if (!File.Exists(Path.Combine(Program.Configuration.ModsPath, "enabled.json")))
            {
                Program.WriteAndClear("No \"enabled.json\" file found in your Mods folder!");
                return;
            }

            string[] mods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Path.Combine(Program.Configuration.ModsPath, "enabled.json")));
            int modCount = 0;
            foreach (string modName in mods)
            {
                modCount++;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($" [{modCount}]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" - {modName}");
            }

            Program.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}
