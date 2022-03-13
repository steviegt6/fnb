using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using Spectre.Console;
using TML.Patcher.Tasks;

namespace TML.Patcher.Client.Commands.Tasks
{
    [Command("extract", Description = "Extracts a .tmod file.")]
    public class ExtractModCommand : InputOutputCommandBase
    {
        [CommandOption("threads", Description = "Specify the amount of threads to use.")]
        public double? Threads { get; set; }

        protected override async ValueTask ExecuteAsync()
        {
            AnsiConsole.MarkupLine($"[gray]Using mod file at path:[/] {PathOverride}");
            AnsiConsole.MarkupLine($"[gray]Using beta:[/] {Beta ??= Program.Runtime!.ProgramConfig.UseBeta}");
            AnsiConsole.MarkupLine($"[gray]Using output path:[/] {OutputOverride}");
            AnsiConsole.MarkupLine($"[gray]Using threads:[/] {Threads ??= Program.Runtime!.ProgramConfig.Threads}");

            if (Beta.Value)
                AnsiConsole.WriteLine(
                    "\n[yellow]WARNING: WORKSHOP MODS DO NOT APPEAR IN THE MOD SELECTION MENU, YOU WILL HAVE TO SPECIFY A PATH MANUALLY" +
                    "\nANY DISPLAYED MODS ARE ONES BUILT OR DOWNLOADED LOCALLY"
                );

            DirectoryInfo outputDir = new(OutputOverride);

            if (outputDir.Exists)
            {
                AnsiConsole.MarkupLine("[gray]\nDeleting previous files, this may take a moment.[/]");
                outputDir.Delete(true);
            }

            outputDir.Create();
            
            AnsiConsole.MarkupLine("\n[gray]Beginning extraction process, this may take some time.\n[/]");

            UnpackTask task = new(
                outputDir,
                PathOverride,
                Threads.Value
            );

            task.ProgressReporter.OnReport += ListenToNotification;

            await task.ExecuteAsync();
        }

        protected override void HandleNullPath()
        {
            DirectoryInfo dir = new(Path.Combine(Program.Runtime!.ProgramConfig.GetStoragePath(Beta), "Mods"));
            Dictionary<string, string> resolvedMods = dir
                .EnumerateFiles("*.tmod")
                .ToDictionary(
                    file => Path.GetFileNameWithoutExtension(file.Name), file => file.FullName
                );
                
            AnsiConsole.MarkupLine($"Resolved [white]{resolvedMods.Count}[/] mod files.\n");

            PathOverride = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[yellow]Mod Selection[/]")
                .AddChoices(resolvedMods.Keys)
                .PageSize(7)
                .MoreChoicesText("[gray]Scroll up/down with the arrow keys to view more files![/]"));
            PathOverride = resolvedMods[PathOverride];
        }

        protected override void HandleNullOutput() =>
            OutputOverride = Program.Runtime!.PlatformStorage.GetFullPath(
                Path.Combine("Extracted", Path.GetFileNameWithoutExtension(PathOverride))
            );
    }
}