using System;
using System.Collections.Generic;
using System.IO;
using Consolation.Common.Framework.OptionsSystem;
using Newtonsoft.Json;

namespace TML.Patcher.Common.Options
{
    public class ListEnabledModsOption : ConsoleOption
    {
        public override string Text => "List all enabled tML mods.";

        public override void Execute()
        {
            if (!File.Exists(Path.Combine(Program.Configuration.ModsPath, "enabled.json")))
            {
                Program.Instance.WriteAndClear("No \"enabled.json\" file found in your Mods folder!");
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

            Program.Instance.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}
