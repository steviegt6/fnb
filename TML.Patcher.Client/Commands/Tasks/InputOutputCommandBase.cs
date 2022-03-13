using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Spectre.Console;

namespace TML.Patcher.Client.Commands.Tasks
{
    public abstract class InputOutputCommandBase : ICommand
    {
        [CommandOption("path", Description = "Manually specifies the file path to use.")]
        public string PathOverride { get; set; } = null!;

        [CommandOption("output", Description = "Manually specifies the output path to use.")]
        public string OutputOverride { get; set; } = null!;

        [CommandOption("beta", Description = "Manually specifies whether this is for the tModLoader Alpha.")]
        public bool? Beta { get; set; }

        async ValueTask ICommand.ExecuteAsync(IConsole console)
        {
            Beta ??= Program.Runtime!.ProgramConfig.UseBeta;
            
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