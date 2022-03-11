using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json;

namespace TML.Patcher.Client.Commands.Informative
{
    [Command(
        "enabled-mods",
        Description = "Lists enabled mods through file parsing. Optionally, lists all resolved mods."
    )]
    public class EnabledModsCommand : ICommand
    {
        [CommandOption(
            "path-override",
            'p',
            Description = "Overrides the default path used. Reads from the local directory."
        )]
        public string? PathOverride { get; init; } = null;

        [CommandOption(
            "list-all",
            'l',
            Description = "List all resolved files. Unresolved files and enabled files will be annotated accordingly."
        )]
        public bool ListAll { get; init; } = false;

        private List<(string mod, bool enabled, bool unresolved)> ModList = new();

        private int PrintCount = 0;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            string path = PathOverride ?? Path.Combine(Program.Runtime!.ProgramConfig.GetStoragePath(), "Mods");

            if (PathOverride is not null)
                await console.Output.WriteLineAsync("Path overriden, using: " + path);
            else
                await console.Output.WriteLineAsync("Using default path: " + path);

            if (!File.Exists(Path.Combine(path, "enabled.json")))
                throw new Exception("Could not find enabled.json at: " + path);

            // Overridden paths are expected to be full paths, if they aren't then that's the end-user's problem.
            List<string> enabledJson = JsonConvert.DeserializeObject<List<string>>(
                await File.ReadAllTextAsync(Path.Combine(path, "enabled.json"))
            ) ?? throw new Exception(
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

            foreach (string enabledMod in from enabledMod in enabledJson
                let resolved =
                    ModList.Any<(string, bool, bool)>(((string mod, bool enabled, bool unresolved) listItem) =>
                        enabledMod == listItem.mod)
                where !resolved
                select enabledMod)
            {
                ModList.Add((enabledMod, true, true));
            }

            foreach ((string modName, bool enabled, bool resolved) in ModList)
            {
                await PrintMod(console, modName, ListAll, enabled, resolved);
            }
        }

        private async Task PrintMod(IConsole console, string modName, bool extra, bool enabled = false,
            bool unresolved = false)
        {
            PrintCount++;

            await console.Output.WriteAsync(' ');
            console.ForegroundColor = ConsoleColor.Yellow;

            string numericExpression = $"[{PrintCount}]";

            await console.Output.WriteAsync(numericExpression);
            
            console.ForegroundColor = ConsoleColor.White;

            for (int _ = 0; _ < 5 - numericExpression.Length; _++)
                await console.Output.WriteAsync(' ');

            await console.Output.WriteAsync($" {modName}");

            if (extra && enabled)
            {
                console.ForegroundColor = ConsoleColor.Green;
                await console.Output.WriteAsync(" [Enabled]");
            }

            if (extra && unresolved)
            {
                console.ForegroundColor = ConsoleColor.Red;
                await console.Output.WriteAsync(" [Unresolved]");
            }

            console.ForegroundColor = ConsoleColor.White;
            await console.Output.WriteAsync('\n');
        }
    }
}