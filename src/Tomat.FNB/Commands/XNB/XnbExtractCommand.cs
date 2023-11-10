using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Tomat.FNB.Commands.XNB;

[Command("xnb extract", Description = "Extracts an XNB file or directory or XNB files into their original formats")]
public class XnbExtractCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
