using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using ICSharpCode.Decompiler.CSharp;
using Spectre.Console;
using TML.Patcher.Tasks;

namespace TML.Patcher.Client.Commands.Tasks
{
    [Command("decompile", Description = "Decompiles a mod assembly.")]
    public class DecompileModCommand : InputOutputCommandBase
    {
        protected override async ValueTask ExecuteAsync()
        {
            if (OutputOverride.EndsWith(".XNA"))
                OutputOverride = OutputOverride[..^".XNA".Length];
            
            AnsiConsole.MarkupLine($"[gray]Using folder at path:[/] {PathOverride}");
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

            List<string> searchDirectories = new()
            {
                Program.Runtime!.ProgramConfig.SteamPath,
                Program.Runtime.ProgramConfig.GetStoragePath(Beta),
                Path.Combine(Program.Runtime.ProgramConfig.GetStoragePath(Beta), "references")
            };

            DirectoryInfo libDir = new(Path.Combine(Path.GetDirectoryName(PathOverride) ?? "", "lib"));
            
            if (libDir.Exists)
                searchDirectories.Add(libDir.FullName);

            DecompilationTask task = new(
                PathOverride,
                outputDir.FullName,
                Beta!.Value ? LanguageVersion.Latest : LanguageVersion.CSharp7_3,
                searchDirectories.ToArray()
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

            if (Beta!.Value)
                PathOverride = Path.Combine(PathOverride, Path.GetFileNameWithoutExtension(PathOverride) + ".dll");
            else
            {
                PathOverride = File.Exists(Path.Combine(PathOverride, "Windows.dll")) 
                    ? Path.Combine(PathOverride, "Windows.dll") 
                    : Path.Combine(PathOverride, Path.GetFileNameWithoutExtension(PathOverride) + ".XNA.dll");
            }
        }

        protected override void HandleNullOutput() =>
            OutputOverride = Program.Runtime!.PlatformStorage.GetFullPath(
                Path.Combine("Decompiled", Path.GetFileNameWithoutExtension(PathOverride))
            );
    }
}