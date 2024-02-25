using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using Tomat.FNB.Commands.TMOD.Abstract;

namespace Tomat.FNB.Commands.TMOD;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[Command("tmod extract-local", Description = "Extracts a mod installed locally in your tModLoader installation")]
public sealed class TmodExtractLocalCommand : TmodAbstractExtractCommand {
    #region Options
    /// <summary>
    ///     The .tmod file name, with or without the .tmod extension.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(0, Name = "name", Description = "The .tmod file name, with or without the .tmod extension", IsRequired = true)]
    public string TmodName { get; set; } = null!;

    /// <summary>
    ///     The branch the mod is installed to, if multiple files with the same
    ///     name exist (tModLoader, ModLoader, tModLoader-1.4.3, etc.).
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(1, Name = "branch", Description = "The branch the mod is installed to, if multiple files with the same name exist (tModLoader, ModLoader, tModLoader-1.4.3, etc.)", IsRequired = false)]
    public string? TmodBranch { get; set; }
    #endregion

    public override async ValueTask ExecuteAsync(IConsole console) {
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
                await ExtractArchive(console, archives.Values.Single(), OutDir);
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

                await ExtractArchive(console, archivePath, OutDir);
                return;
        }
    }
}
