using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.TMOD;

[Command("tmod list-locals", Description = "Lists known, locally-installed .tmod files")]
public class TmodListLocalsCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
