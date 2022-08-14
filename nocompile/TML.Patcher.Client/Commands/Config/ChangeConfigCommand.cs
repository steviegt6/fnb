using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Spectre.Console;
using TML.Patcher.Client.Configuration;

namespace TML.Patcher.Client.Commands.Config
{
    [Command("config", Description = "Configure client functionality.")]
    public class ChangeConfigCommand : ICommand
    {
        [CommandOption("storage-path", Description = "Sets the game storage path (where worlds, players, etc. are),")]
        public string? NewStoragePath { get; init; } = null;

        [CommandOption("steam-path", Description = "Sets the Steam/GOG game path.")]
        public string? NewSteamPath { get; init; } = null;
        
        [CommandOption("tml-version", Description = "Changes the default tModLoader version.")]
        public string? LoaderVersion { get; init; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (NewStoragePath is not null)
            {
                Program.Runtime!.ProgramConfig.StoragePath = NewStoragePath;
                await console.Output.WriteLineAsync("Set game storage path to: " + NewSteamPath);
            }

            if (NewSteamPath is not null)
            {
                Program.Runtime!.ProgramConfig.SteamPath = NewSteamPath;
                await console.Output.WriteLineAsync("Set Steam path to: " + NewSteamPath);
            }

            if (LoaderVersion is not null)
            {
                ModLoaderVersion realVersion = ProgramConfig.GetVersionFromName(LoaderVersion, out bool valid);
                Program.Runtime!.ProgramConfig.LoaderVersion = realVersion.VersionAliases[0];
                
                await console.Output.WriteLineAsync($"Using tModLoader version: {realVersion.VersionAliases[1]}");
                
                if (!valid)
                {
                    AnsiConsole.MarkupLine("[yellow]WARNING: The input used for the tModLoader version was not valid." +
                                           "\nThe default version \"legacy\" (1.3) is being used." +
                                           "\nBelow is a list of valid versions:\n[/]");
                    ProgramConfig.PrintVersions();
                }
            }
            
            ProgramConfig.SerializeConfig(Program.Runtime!.ProgramConfig, Program.Runtime.PlatformStorage);
        }
    }
}