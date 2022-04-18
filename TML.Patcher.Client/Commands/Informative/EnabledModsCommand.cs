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
using TML.Patcher.Client.Configuration;

namespace TML.Patcher.Client.Commands.Informative
{
    [Command(
        "enabled-mods",
        Description = "Lists enabled mods through file parsing. Optionally, lists all resolved mods."
    )]
    public class EnabledModsCommand : ICommand
    {
        [CommandOption("path-override", Description = "Manually specifies the input path to use.")]
        public string? PathOverride { get; set; }
        
        [CommandOption("workshop-override", Description = "Manually specifies the base workshop path to use.")]
        public string? WorkshopOverride { get; set; }

        [CommandOption("enabled-override", Description = "Manually specifies the path of the enabled.json file.")]
        public string? EnabledOverride { get; set; }

        [CommandOption("list-all", Description = "List all resolved mods.")]
        public bool ListAll { get; init; }

        [CommandOption("descriptive", Description = "Adds additional annotations, indicating resolution status, etc.")]
        public bool Descriptive { get; init; }
        
        [CommandOption("tml-version", Description = "Describes the tModLoader version.")]
        public string? LoaderVersion { get; set; }
        
        private readonly List<(string mod, bool enabled, bool unresolved, bool? local)> ModList = new();

        private int PrintCount;

        protected ModLoaderVersion VersionToUse;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            LoaderVersion ??= Program.Runtime!.ProgramConfig.LoaderVersion;

            VersionToUse = ProgramConfig.GetVersionFromName(LoaderVersion, out bool valid);

            if (!valid)
            {
                AnsiConsole.MarkupLine("[yellow]WARNING: The input used for the tModLoader version was not valid." +
                                       "\nThe default version \"legacy\" (1.3) is being used." +
                                       "\nBelow is a list of valid versions:\n[/]");
                ProgramConfig.PrintVersions();
            }
            
            PathOverride ??= Path.Combine(Program.Runtime!.ProgramConfig.GetStoragePath(VersionToUse), "Mods");
            EnabledOverride ??= Path.Combine(PathOverride, "enabled.json");
            
            // ../../workshop...
            WorkshopOverride ??= Path.Combine(
                Program.Runtime!.ProgramConfig.SteamPath,
                "..",
                "..",
                "workshop",
                "content",
                "1281930"
            );

            AnsiConsole.MarkupLine($"[gray]Using folder at path:[/] {PathOverride}");
            AnsiConsole.MarkupLine($"[gray]Using local mods folder at path:[/] {PathOverride}");
            AnsiConsole.MarkupLine($"[gray]Using enabled.json path:[/] {EnabledOverride}");
            
            if (VersionToUse.Workshop)
                AnsiConsole.MarkupLine($"[gray]Using base workshop path:[/] {WorkshopOverride}");

            AnsiConsole.MarkupLine($"[gray]Listing all resolved mods:[/] {ListAll}");
            AnsiConsole.MarkupLine($"[gray]Descriptive mod annotations:[/] {Descriptive}");
            AnsiConsole.MarkupLine($"[gray]Using tModLoader version:[/] {VersionToUse.VersionAliases[1]}\n");

            if (!File.Exists(EnabledOverride))
                throw new FileNotFoundException("Could not find enabled.json at: " + EnabledOverride);

            // Overridden paths are expected to be full paths, if they aren't then that's the user's problem.
            List<string> enabledJson = JsonConvert.DeserializeObject<List<string>>(
                await File.ReadAllTextAsync(EnabledOverride)
            ) ?? throw new JsonReaderException(
                "Failed to deserialize enabled.json as a list of strings!"
            );

            if (VersionToUse.Workshop && ListAll)
            {
                DirectoryInfo workshopDir = new(WorkshopOverride);

                if (!workshopDir.Exists)
                    throw new DirectoryNotFoundException($"Could not resolve workshop directory: {workshopDir}");
                
                foreach (DirectoryInfo modDir in workshopDir.EnumerateDirectories())
                foreach (FileInfo modFile in modDir.EnumerateFiles("*.tmod"))
                {
                    string modName = Path.GetFileNameWithoutExtension(modFile.FullName);
                    bool enabled = enabledJson.Contains(modName);
                    
                    ModList.Add((modName, enabled, false, false));
                }
            }

            if (ListAll)
                foreach (string tmodFile in Directory.GetFiles(PathOverride))
                {
                    if (Path.GetExtension(tmodFile) != ".tmod")
                        continue;

                    string modName = Path.GetFileNameWithoutExtension(tmodFile);
                    bool enabled = enabledJson.Contains(modName);

                    ModList.Add((modName, enabled, false, true));
                }

            foreach (string enabledMod in enabledJson)
            {
                bool resolved = ModList.Any<(string, bool, bool, bool?)>(
                    ((string mod, bool enabled, bool unresolved, bool? local) listItem) => enabledMod == listItem.mod
                );

                if (!resolved)
                    ModList.Add((enabledMod, true, true, null));
            }

            foreach ((string modName, bool enabled, bool resolved, bool? local) in ModList)
                PrintMod(modName, Descriptive, ListAll, enabled, resolved, local);
        }

        private void PrintMod(
            string modName,
            bool extra,
            bool listAll,
            bool enabled = false,
            bool unresolved = false,
            bool? local = null
        )
        {
            PrintCount++;

            AnsiConsole.Markup(" ");

            string numericExpression = $"{PrintCount}";

            AnsiConsole.Markup($"[yellow][[{PrintCount}]][/]");

            for (int _ = 0; _ < 3 - numericExpression.Length; _++)
                AnsiConsole.Write(" ");

            AnsiConsole.Markup($" [white]{modName}[/]");
            
            if (local is not null)
                AnsiConsole.Markup($" [gray][[{(local.Value ? "Local" : "Workshop")}]][/]");

            if (extra && enabled) 
                AnsiConsole.Markup(" [green][[Enabled]][/]");

            if (extra && unresolved && listAll)
                AnsiConsole.Markup(" [red][[Unresolved]][/]");

            AnsiConsole.WriteLine();
        }
    }
}