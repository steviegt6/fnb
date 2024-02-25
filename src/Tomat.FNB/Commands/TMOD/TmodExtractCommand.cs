using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using Tomat.FNB.Commands.TMOD.Abstract;

namespace Tomat.FNB.Commands.TMOD;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[Command("tmod extract", Description = "Extracts a .tmod file archive into a directory")]
public sealed class TmodExtractCommand : TmodAbstractExtractCommand {
    #region Options
    /// <summary>
    ///     The .tmod file path.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(0, Name = "path", Description = "The .tmod file path", IsRequired = true)]
    public string TmodPath { get; set; } = null!;
    #endregion

    public override async ValueTask ExecuteAsync(IConsole console) {
        if (!System.IO.File.Exists(TmodPath)) {
            if (!System.IO.File.Exists(TmodPath + ".tmod")) {
                await console.Output.WriteLineAsync($"No .tmod file found at \"{TmodPath}\".");
                return;
            }

            TmodPath += ".tmod";
        }

        await ExtractArchive(console, TmodPath, OutDir);
    }
}
