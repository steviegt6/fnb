using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.TMOD;

[Command("tmod extract-file", Description = "Extracts a single file from a .tmod file archive")]
public class TmodExtractFileCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
