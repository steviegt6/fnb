using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using ICSharpCode.Decompiler.CSharp;
using Spectre.Console;
using TML.Patcher.Tasks;

namespace TML.Patcher.Client.Commands.Tasks
{
    [Command("extract", Description = "Extracts a .tmod file.")]
    public class ExtractModCommand : ICommand
    {
        [CommandOption("path", Description = "Manually input the .tmod file path.")]
        public string? PathOverride { get; set; }

        [CommandOption("output", Description = "Manually input the output path.")]
        public string? OutputOverride { get; set; }

        [CommandOption("beta", Description = "Manually specify whether this is for the tModLoader Alpha.")]
        public bool? Beta { get; set; }

        [CommandOption("threads", Description = "Specify the amount of threads to use.")]
        public double? Threads { get; set; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (PathOverride is null)
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

            OutputOverride ??= Program.Runtime!.PlatformStorage.GetFullPath(
                Path.Combine("Extracted", Path.GetFileNameWithoutExtension(PathOverride))
            );
            
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
                // Beta.Value ? LanguageVersion.Latest : LanguageVersion.CSharp7_3,
                // Program.Runtime!.ProgramConfig.SteamPath,
                // Program.Runtime.ProgramConfig.GetStoragePath(Beta)
            );

            task.ProgressReporter.OnReport += notification =>
            {
                AnsiConsole.WriteLine(notification.Progressive
                    ? $"{notification.Status} ({notification.Current}/{notification.Total})"
                    : notification.Status);
            };

            await task.ExecuteAsync();
        }
    }
}