using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json;
using Spectre.Console;

namespace TML.Patcher.Client.Commands.Informative
{
    [Command(
        "enabled-mods",
        Description = "Lists enabled mods through file parsing. Optionally, lists all resolved mods."
    )]
    public class EnabledModsCommand : ICommand
    {
        [CommandOption("path-override", 'p', Description = "Manually specifies the input path to use.")]
        public string? PathOverride { get; init; }

        [CommandOption(
            "list-all",
            'l',
            Description = "List all resolved files. Unresolved files and enabled files will be annotated accordingly."
        )]
        public bool ListAll { get; init; }

        private readonly List<(string mod, bool enabled, bool unresolved)> ModList = new();

        private int PrintCount;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            string path = PathOverride ?? Path.Combine(Program.Runtime!.ProgramConfig.GetStoragePath(), "Mods");

            AnsiConsole.MarkupLine($"[gray]Using folder at path:[/] {path}");
            
            if (!File.Exists(Path.Combine(path, "enabled.json")))
                throw new FileNotFoundException("Could not find enabled.json at: " + path);

            // Overridden paths are expected to be full paths, if they aren't then that's the user's problem.
            List<string> enabledJson = JsonConvert.DeserializeObject<List<string>>(
                await File.ReadAllTextAsync(Path.Combine(path, "enabled.json"))
            ) ?? throw new JsonReaderException(
                "Failed to deserialize enabled.json as a list of strings!"
            );

            if (ListAll)
                foreach (string tmodFile in Directory.GetFiles(path))
                {
                    if (Path.GetExtension(tmodFile) != ".tmod")
                        continue;

                    string modName = Path.GetFileNameWithoutExtension(tmodFile);
                    bool enabled = enabledJson.Contains(modName);

                    ModList.Add((modName, enabled, false));
                }

            foreach (string enabledMod in enabledJson)
            {
                bool resolved = ModList.Any<(string, bool, bool)>(
                    ((string mod, bool enabled, bool unresolved) listItem) => enabledMod == listItem.mod
                );

                if (!resolved)
                    ModList.Add((enabledMod, true, true));
            }

            foreach ((string modName, bool enabled, bool resolved) in ModList)
                PrintMod(console, modName, ListAll, enabled, resolved);
        }

        private void PrintMod(
            IConsole console,
            string modName,
            bool extra,
            bool enabled = false,
            bool unresolved = false
        )
        {
            PrintCount++;

            AnsiConsole.Markup(" ");
            console.ForegroundColor = ConsoleColor.Yellow;

            string numericExpression = $"{PrintCount}";

            AnsiConsole.Markup($"[yellow][[{PrintCount}]][/]");

            for (int _ = 0; _ < 5 - numericExpression.Length; _++)
                AnsiConsole.Write(" ");

            AnsiConsole.Markup($" [white]{modName}[/]");

            if (extra && enabled) 
                AnsiConsole.Markup(" [green][[Enabled]][/]");

            if (extra && unresolved)
                AnsiConsole.Markup(" [red][[Unresolved]][/]");

            AnsiConsole.WriteLine();
        }
    }
}