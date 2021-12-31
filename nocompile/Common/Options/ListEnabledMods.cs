using System;
using System.IO;
using Consolation.Framework.OptionsSystem;
using Newtonsoft.Json;

namespace TML.Patcher.CLI.Common.Options
{
    /// <summary>
    ///     Lists your enabled tML mods.
    /// </summary>
    public class ListEnabledModsOption : ConsoleOption
    {
        /// <inheritdoc cref="ConsoleOption.Text"/>
        public override string Text => "List all enabled tML mods.";

        /// <summary>
        ///     Writes the listed mods to the console as a paged list.
        /// </summary>
        public override void Execute()
        {
            Patcher window = Program.Patcher;

            if (!File.Exists(Path.Combine(Program.Configuration.ModsPath, "enabled.json")))
            {
                window.WriteAndClear("No \"enabled.json\" file found in your Mods folder!");
                Program.Patcher.SelectedOptions = window.DefaultOptions;
                Program.Patcher.SelectedOptions.ListForOption(Program.Patcher);
            }
            else
            {
                while (true)
                {
                    string[]? mods =
                        JsonConvert.DeserializeObject<string[]>(
                            File.ReadAllText(Path.Combine(Program.Configuration.ModsPath, "enabled.json")));

                    if (mods == null)
                        break;

                    window.WriteAndClear("Displaying mods detected as enabled in enabled.json.", ConsoleColor.Yellow);

                    int modCount = 0;
                    foreach (string modName in mods)
                    {
                        modCount++;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write($" [{modCount}]");
                        Console.ForegroundColor = ConsoleColor.White;
                        window.WriteLine($" - {modName}");
                    }

                    break;
                }

                window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
            }
        }
    }
}