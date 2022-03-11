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
    [Command("decompile", Description = "Decompiles a mod assembly.")]
    public class DecompileModCommand : ICommand
    {
        [CommandOption("path", Description = "Manually input the .tmod file path.")]
        public string? PathOverride { get; set; }

        [CommandOption("output", Description = "Manually input the output path.")]
        public string? OutputOverride { get; set; }

        [CommandOption("beta", Description = "Manually specify whether this is for the tModLoader Alpha.")]
        public bool? Beta { get; set; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            Beta ??= Program.Runtime!.ProgramConfig.UseBeta;
            
            if (PathOverride is null)
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

                if (Beta.Value)
                    PathOverride = Path.Combine(PathOverride, Path.GetFileNameWithoutExtension(PathOverride) + ".dll");
                else
                {
                    PathOverride = File.Exists(Path.Combine(PathOverride, "Windows.dll")) 
                        ? Path.Combine(PathOverride, "Windows.dll") 
                        : Path.Combine(PathOverride, Path.GetFileNameWithoutExtension(PathOverride) + ".XNA.dll");
                }
            }

            OutputOverride ??= Program.Runtime!.PlatformStorage.GetFullPath(
                Path.Combine("Decompiled", Path.GetFileNameWithoutExtension(PathOverride))
            );

            if (OutputOverride.EndsWith(".XNA"))
                OutputOverride = OutputOverride[..^".XNA".Length];
            
            AnsiConsole.MarkupLine($"[gray]Using mod file at path:[/] {PathOverride}");
            AnsiConsole.MarkupLine($"[gray]Using beta:[/] {Beta}");
            AnsiConsole.MarkupLine($"[gray]Using output path:[/] {OutputOverride}");

            DirectoryInfo outputDir = new(OutputOverride);

            if (outputDir.Exists)
            {
                AnsiConsole.MarkupLine("[gray]Deleting previous files, this may take a moment.[/]");
                outputDir.Delete(true);
            }

            outputDir.Create();
            
            AnsiConsole.MarkupLine("\n[gray]Beginning extraction process, this may take some time.\n[/]");

            DecompilationTask task = new(
                PathOverride,
                outputDir.FullName,
                Beta.Value ? LanguageVersion.Latest : LanguageVersion.CSharp7_3,
                Program.Runtime!.ProgramConfig.SteamPath,
                Program.Runtime.ProgramConfig.GetStoragePath(Beta)
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