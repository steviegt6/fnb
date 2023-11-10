using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;

namespace Tomat.FNB.Commands.TMOD;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[Command("tmod extract-local", Description = "Extracts a mod installed locally in your tModLoader installation")]
public class TmodExtractLocalCommand : ICommand {
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(0, Name = "name", Description = "The .tmod file name, with or without the .tmod extension", IsRequired = true)]
    public string TmodName { get; set; } = null!;

    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(1, Name = "branch", Description = "The branch the mod is installed to, if multiple files with the same name exist (tModLoader, ModLoader, tModLoader-1.4.3, etc.)", IsRequired = false)]
    public string? TmodBranch { get; set; }

    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("out-dir", 'o', Description = "The directory to extract the mod to. If not specified, it will be extracted to ./<mod name>", IsRequired = false)]
    public string? OutputDirectory { get; set; }

    public async ValueTask ExecuteAsync(IConsole console) {
        if (!CommandUtil.TryGetLocalTmodArchives(out var localMods)) {
            await console.Output.WriteLineAsync("No local mods found or local installation directory could not be resolved.");
            return;
        }

        var archives = new Dictionary<string, string>();

        foreach (var (branchName, branchArchives) in localMods) {
            foreach (var (archiveName, archivePath) in branchArchives) {
                if (archiveName == TmodName || archiveName == $"{TmodName}.tmod")
                    archives.Add(branchName, archivePath);
            }
        }

        switch (archives.Count) {
            case 0:
                await console.Output.WriteLineAsync($"No local mods found with the name \"{TmodName}\".");
                return;

            case 1:
                await CommandUtil.ExtractArchive(console, archives.Values.Single(), OutputDirectory);
                return;

            default:
                if (TmodBranch is null) {
                    await console.Output.WriteLineAsync($"Multiple local mods found with the name \"{TmodName}\". Please specify a branch to extract from.");
                    return;
                }

                if (!archives.TryGetValue(TmodBranch, out var archivePath)) {
                    await console.Output.WriteLineAsync($"No local mods found with the name \"{TmodName}\" and branch \"{TmodBranch}\".");
                    return;
                }

                await CommandUtil.ExtractArchive(console, archivePath, OutputDirectory);
                return;
        }
    }
}
