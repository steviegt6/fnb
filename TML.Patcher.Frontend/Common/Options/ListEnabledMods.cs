using System;
using System.IO;
using Consolation.Common.Framework.OptionsSystem;
using Newtonsoft.Json;

namespace TML.Patcher.Frontend.Common.Options
{
    public class ListEnabledModsOption : ConsoleOption
    {
        public override string Text => "List all enabled tML mods.";

        public override void Execute()
        {
            Patcher window = Consolation.Consolation.GetWindow<Patcher>();

            if (!File.Exists(Path.Combine(Program.Configuration.ModsPath, "enabled.json")))
            {
                window.WriteAndClear("No \"enabled.json\" file found in your Mods folder!");
                Consolation.Consolation.SelectedOptionSet = window.DefaultOptions;
                Consolation.Consolation.SelectedOptionSet.ListForOption();
            }
            else
            {
                string[]? mods =
                    JsonConvert.DeserializeObject<string[]>(
                        File.ReadAllText(Path.Combine(Program.Configuration.ModsPath, "enabled.json")));

                if (mods == null)
                    goto SkipIfNull;

                window.WriteAndClear("Displaying mods detected as enabled in enabled.json.", ConsoleColor.Yellow);

                int modCount = 0;
                foreach (string modName in mods)
                {
                    modCount++;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" [{modCount}]");
                    Console.ForegroundColor = ConsoleColor.White;
                    window.WriteLine(0, $" - {modName}");
                }

                SkipIfNull:
                window.WriteOptionsList(new ConsoleOptions("Return:"));
            }
        }
    }
}