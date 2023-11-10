using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.TMOD;

[Command("tmod extract-local", Description = "Extracts a mod installed locally in your tModLoader installation")]
public class TmodExtractLocalCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
