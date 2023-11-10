using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.XNB;

[Command("xnb pack", Description = "Packs a file or directory of files into XNB files")]
public class XnbPackCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
