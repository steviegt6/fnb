using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using TML.Patcher.CLI.Configuration;

namespace TML.Patcher.CLI.Commands.Config
{
    [Command]
    public class ChangeConfigCommand : ICommand
    {
        [CommandOption("storage-path", Description = "Sets the game storage path (where worlds, players, etc. are),")]
        public string? NewStoragePath { get; init; } = null;

        [CommandOption("steam-path", Description = "Sets the Steam/GOG game path.")]
        public string? NewSteamPath { get; init; } = null;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (NewStoragePath is not null)
            {
                Program.Runtime!.ProgramConfig.StoragePath = NewStoragePath;
                await console.Output.WriteLineAsync("Set game storage to: " + NewSteamPath);
            }

            if (NewSteamPath is not null)
            {
                Program.Runtime!.ProgramConfig.SteamPath = NewSteamPath;
                await console.Output.WriteLineAsync("Set steam path to: " + NewSteamPath);
            }
            
            ProgramConfig.SerializeConfig(Program.Runtime!.ProgramConfig, Program.Runtime.PlatformStorage);
        }
    }
}