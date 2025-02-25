using System;
using System.IO;
using System.Threading.Tasks;

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

using JetBrains.Annotations;

using Tomat.FNB.CLI.Commands.TMOD.Abstract;

namespace Tomat.FNB.CLI.Commands.TMOD;

[Command("tmod extract", Description = "Extracts a .tmod file archive to a directory")]
public sealed class TmodExtractCommand : AbstractTmodExtractCommand
{
#region Options
    [CommandParameter(
        0,
        Name = "path",
        Description = "Path to the .tmod file",
        IsRequired = true
    )]
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public string TmodPath { get; set; } = string.Empty;
#endregion

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        if (!File.Exists(TmodPath))
        {
            // We can also try appending '.tmod'.

            if (!File.Exists(TmodPath + ".tmod"))
            {
                await console.Error.WriteLineAsync("The .tmod file was not found: " + TmodPath);
                Environment.ExitCode = 1;
                return;
            }

            TmodPath += ".tmod";
        }

        await ExtractArchive(console, TmodPath, OutputPath);
    }
}