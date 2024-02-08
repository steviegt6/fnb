using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;

namespace Tomat.FNB.Commands.XNB;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
[Command("xnb extract", Description = "Extracts an XNB file or directory or XNB files into their original formats")]
public sealed class XnbExtractCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new NotImplementedException();
    }
}
