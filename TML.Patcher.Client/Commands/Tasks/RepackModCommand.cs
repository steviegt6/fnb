using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using Spectre.Console;
using TML.Patcher.Tasks;

namespace TML.Patcher.Client.Commands.Tasks
{
    [Command("repack", Description = "Repacks an extracted .tmod file. (UNSUPPORTED)")]
    public class RepackModCommand : InputOutputCommandBase
    {
        [CommandOption("mod-name", Description = "The name to use when this mod is repacked.")]
        public string? ModName { get; init; }

        [CommandOption("mod-version", Description = "The version to use when this mod is repacked.")]
        public string? ModVersion { get; init; }

        [CommandOption("modloader-version", Description = "The tModLoader version to use whe this mod is repkaced.")]
        public string? ModLoaderVersion { get; init; }
        
        [CommandOption("threads", Description = "Specify the amount of threads to use.")]
        public double? Threads { get; set; }

        protected override async ValueTask ExecuteAsync()
        {
            if (!Debugger.IsAttached)
                throw new InvalidOperationException(
                    "Repacking is not currently supported, please compile an extracted folder in-game."
                );

            Directory.CreateDirectory(Path.GetDirectoryName(OutputOverride) ?? "");

            string? modName = ModName;
            string? modVersion = ModVersion;
            string? modLoaderVersion = ModLoaderVersion;
            
            RequestInput(ref modName, "Please enter the mod's display name:");
            RequestInput(ref modVersion, "Please enter the mod's version:");
            RequestInput(ref modLoaderVersion, "Please enter the tModLoader version:");
            
            AnsiConsole.MarkupLine($"[gray]Using folder at path:[/] {PathOverride}");
            AnsiConsole.MarkupLine($"[gray]Using beta:[/] {Beta}");
            AnsiConsole.MarkupLine($"[gray]Using output path:[/] {OutputOverride}");
            AnsiConsole.MarkupLine($"[gray]Using threads:[/] {Threads ??= Program.Runtime!.ProgramConfig.Threads}");

            FileInfo outputFile = new(OutputOverride);

            if (outputFile.Exists)
            {
                AnsiConsole.MarkupLine("[gray]\nDeleting previous file.[/]");
                outputFile.Delete();
            }
            
            AnsiConsole.MarkupLine("\n[gray]Beginning packing process, this may take some time.\n[/]");

            RepackTask task = new(
                new DirectoryInfo(PathOverride),
                OutputOverride,
                modName!,
                modVersion!,
                modLoaderVersion!,
                Threads.Value
            );

            task.ProgressReporter.OnReport += ListenToNotification;

            await task.ExecuteAsync();
        }

        protected override void HandleNullPath()
        {
            DirectoryInfo dir = new(Path.Combine(Program.Runtime!.PlatformStorage.GetFullPath("Extracted")));
                
            dir.Create();
                
            Dictionary<string, string> resolvedMods = dir
                .EnumerateDirectories("**")
                .ToDictionary(
                    file => Path.GetFileNameWithoutExtension(file.Name), file => file.FullName
                );
                
            AnsiConsole.MarkupLine($"Resolved [white]{resolvedMods.Count}[/] extracted mod directories.\n");

            PathOverride = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[yellow]Extracted Mod Selection[/]")
                .AddChoices(resolvedMods.Keys)
                .PageSize(7)
                .MoreChoicesText("[gray]Scroll up/down with the arrow keys to view more folders![/]"));
            PathOverride = resolvedMods[PathOverride];
        }

        protected override void HandleNullOutput() =>
            OutputOverride = Program.Runtime!.PlatformStorage.GetFullPath(
                Path.Combine("Repacked", Path.GetFileNameWithoutExtension(PathOverride) + ".tmod")
            );

        private static void RequestInput(ref string? value, string message) => value ??= AnsiConsole.Ask<string>(message);
    }
}