using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;

namespace Tomat.FNB.Commands.TMOD;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[Command("tmod extract", Description = "Extracts a .tmod file archive into a directory")]
public class TmodExtractCommand : ICommand {
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(0, Name = "path", Description = "The .tmod file path", IsRequired = true)]
    public string TmodPath { get; set; } = null!;

    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("out-dir", 'o', Description = "The directory to extract the mod to. If not specified, it will be extracted to ./<mod name>", IsRequired = false)]
    public string? OutputDirectory { get; set; }

    public async ValueTask ExecuteAsync(IConsole console) {
        if (!File.Exists(TmodPath)) {
            if (!File.Exists(TmodPath + ".tmod")) {
                await console.Output.WriteLineAsync($"No .tmod file found at \"{TmodPath}\".");
                return;
            }

            TmodPath += ".tmod";
        }

        await CommandUtil.ExtractArchive(console, TmodPath, OutputDirectory);
    }
}
