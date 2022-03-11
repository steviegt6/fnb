using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
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
        
        [CommandOption("toggle-beta", Description = "Switch to and from using tModLoader beta paths (for the alpha).")]
        public bool ToggleBeta { get; init; }

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

            if (ToggleBeta)
            {
                Program.Runtime!.ProgramConfig.UseBeta = !Program.Runtime.ProgramConfig.UseBeta;

                if (Program.Runtime.ProgramConfig.UseBeta)
                    await console.Output.WriteLineAsync("Now using the tModLoader beta.");
                else
                    await console.Output.WriteLineAsync("No longer using the tModLoader beta.");
            }
            
            ProgramConfig.SerializeConfig(Program.Runtime!.ProgramConfig, Program.Runtime.PlatformStorage);
        }
    }
}