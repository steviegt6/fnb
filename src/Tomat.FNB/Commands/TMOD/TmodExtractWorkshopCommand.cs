using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.TMOD;

[Command("tmod extract-workshop", Description = "Extracts a mod installed through the Steam Workshop")]
public class TmodExtractWorkshopCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
