using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using Tomat.FNB.Commands.TMOD.Abstract;

namespace Tomat.FNB.Commands.TMOD;

// TODO: Searching based on Workshop entry ID?
// ^ Should not be necessary, modders should not upload mods of the same
// internal name to multiple pages. Should we handle this anyway?

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[Command("tmod extract-workshop", Description = "Extracts a mod installed through the Steam Workshop")]
public sealed class TmodExtractWorkshopCommand : TmodAbstractExtractCommand {
    #region Options
    /// <summary>
    ///     The .tmod file name, with or without the .tmod extension.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(0, Name = "name", Description = "The .tmod file name, with or without the .tmod extension", IsRequired = true)]
    public string TmodName { get; set; } = null!;

    /// <summary>
    ///     The tModLoader major version, latest, earliest, or unversioned
    ///     (ex.: 2022.4, latest, earliest, unversioned).
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandParameter(1, Name = "version", Description = "The tModLoader major version, latest, earliest, or unversioned (ex.: 2022.4, latest, earliest, unversioned)", IsRequired = false)]
    public string? TmodVersion { get; set; }
    #endregion
    
    public override async ValueTask ExecuteAsync(IConsole console) {
        if (!CommandUtil.TryGetWorkshopDirectory(CommandUtil.TMODLOADER_APPID, out var workshopDir)) {
            await console.Output.WriteLineAsync($"Could not locate the Steam Workshop directory for tModLoader (appId: {CommandUtil.TMODLOADER_APPID})");
            return;
        }

        var knownMods = CommandUtil.ResolveTmodWorkshopEntries(workshopDir);
        var record = knownMods.Values.FirstOrDefault(x => x.Items.Any(y => y.TmodName == TmodName || y.TmodName == $"{TmodName}.tmod"));

        if (record is null) {
            await console.Output.WriteLineAsync($"No Steam Workshop mods found with the name \"{TmodName}\".");
            return;
        }

        TmodVersion ??= "latest";
        TmodVersion = TmodVersion.ToLowerInvariant();

        switch (TmodVersion) {
            case "unversioned": {
                var unversioned = record.Items.FirstOrDefault(x => x.Version is null);

                if (unversioned is null) {
                    await console.Output.WriteLineAsync($"No unversioned Steam Workshop mods found with the name \"{TmodName}\".");
                    return;
                }

                await ExtractArchive(console, unversioned.FullPath, OutDir);
                break;
            }

            case "earliest": {
                if (record.Items.Count == 1) {
                    await ExtractArchive(console, record.Items[0].FullPath, OutDir);
                    break;
                }

                var unversioned = record.Items.FirstOrDefault(x => x.Version is null);

                if (unversioned is not null) {
                    await ExtractArchive(console, unversioned.FullPath, OutDir);
                    break;
                }

                var versions = record.Items.Select(x => new Version(x.Version!));
                var earliest = versions.Min();

                var earliestVersion = record.Items.First(x => new Version(x.Version!) == earliest);
                await ExtractArchive(console, earliestVersion.FullPath, OutDir);
                break;
            }

            case "latest": {
                if (record.Items.Count == 1) {
                    await ExtractArchive(console, record.Items[0].FullPath, OutDir);
                    break;
                }

                var versions = record.Items.Select(x => new Version(x.Version!));
                var latest = versions.Max();

                var latestVersion = record.Items.First(x => new Version(x.Version!) == latest);
                await ExtractArchive(console, latestVersion.FullPath, OutDir);
                break;
            }

            default: {
                var version = new Version(TmodVersion);

                var versioned = record.Items.FirstOrDefault(x => new Version(x.Version!) == version);

                if (versioned is null) {
                    await console.Output.WriteLineAsync($"No Steam Workshop mods found with the name \"{TmodName}\" and version \"{TmodVersion}\".");
                    return;
                }

                await ExtractArchive(console, versioned.FullPath, OutDir);
                break;
            }
        }
    }
}
