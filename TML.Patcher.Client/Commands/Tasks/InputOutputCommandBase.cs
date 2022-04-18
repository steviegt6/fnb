using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Spectre.Console;
using TML.Patcher.Client.Configuration;

namespace TML.Patcher.Client.Commands.Tasks
{
    public abstract class InputOutputCommandBase : ICommand
    {
        [CommandOption("path", Description = "Manually specifies the file path to use.")]
        public string PathOverride { get; set; } = null!;

        [CommandOption("output", Description = "Manually specifies the output path to use.")]
        public string OutputOverride { get; set; } = null!;

        [CommandOption("tml-version", Description = "Describes the tModLoader version.")]
        public string? LoaderVersion { get; set; }

        protected ModLoaderVersion VersionToUse;

        async ValueTask ICommand.ExecuteAsync(IConsole console)
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
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (PathOverride is null)
                HandleNullPath();
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (OutputOverride is null)
                HandleNullOutput();

            await ExecuteAsync();
        }

        protected abstract ValueTask ExecuteAsync();

        protected virtual void ListenToNotification(ProgressNotification notification) => AnsiConsole.WriteLine(
            notification.Progressive
                ? $"{notification.Status} ({notification.Current}/{notification.Total})"
                : notification.Status
        );

        protected abstract void HandleNullPath();

        protected abstract void HandleNullOutput();
    }
}