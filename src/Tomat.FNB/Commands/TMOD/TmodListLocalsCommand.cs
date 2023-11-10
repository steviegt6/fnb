using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.TMOD;

[Command("tmod list-locals", Description = "Lists known, locally-installed .tmod files")]
public class TmodListLocalsCommand : ICommand {
    public async ValueTask ExecuteAsync(IConsole console) {
        if (!CommandUtil.TryGetLocalTmodArchives(out var localMods)) {
            await console.Output.WriteLineAsync("No local mods found or local installation directory could not be resolved.");
            return;
        }

        var totalArchives = localMods.Sum(x => x.Value.Count);
        await console.Output.WriteLineAsync($"Found {totalArchives} {(totalArchives == 1 ? "mod" : "mods")} installed locally:");

        foreach (var (branchName, archives) in localMods) {
            await console.Output.WriteLineAsync($"    {branchName} ({archives.Count} {(archives.Count == 1 ? "mod" : "mods")}):");

            foreach (var archive in archives) {
                await console.Output.WriteLineAsync($"        {Path.GetFileName(archive.Value)}");
            }
        }
    }
}
