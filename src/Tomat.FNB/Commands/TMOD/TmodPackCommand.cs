using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.TMOD;

[Command("tmod pack", Description = "Packs a directory into a .tmod file archive")]
public class TmodPackCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
