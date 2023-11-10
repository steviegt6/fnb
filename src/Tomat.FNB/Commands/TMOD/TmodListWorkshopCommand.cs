using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;

namespace Tomat.FNB.Commands.TMOD;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[Command("tmod list-workshop", Description = "Lists .tmod files installed through the Steam Workshop")]
public class TmodListWorkshopCommand : ICommand {
    public async ValueTask ExecuteAsync(IConsole console) {
        if (!CommandUtil.TryGetWorkshopDirectory(CommandUtil.TMODLOADER_APPID, out var workshopDir)) {
            await console.Output.WriteLineAsync($"Could not locate the Steam Workshop directory for tModLoader (appId: {CommandUtil.TMODLOADER_APPID})");
            return;
        }

        var knownMods = CommandUtil.ResolveTmodWorkshopEntries(workshopDir);

        var totalArchives = knownMods.Sum(x => x.Value.Items.Count);
        await console.Output.WriteLineAsync($"Found {knownMods.Count} {(knownMods.Count == 1 ? "mod" : "mods")} ({totalArchives} {(totalArchives == 1 ? "archive" : "archives")}) installed through the Steam Workshop:");

        foreach (var (_, knownMod) in knownMods) {
            await console.Output.WriteLineAsync($"    {knownMod.ItemId} ({knownMod.Items.Count} {(knownMod.Items.Count == 1 ? "archive" : "archives")}):");

            foreach (var item in knownMod.Items) {
                await console.Output.WriteLineAsync($"        {item.TmodName} ({(item.Version is not null ? $"tML v{item.Version}" : "unversioned, pre-2022.4")})");
            }
        }
    }
}
