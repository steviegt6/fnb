using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace TML.Patcher.Client.Commands.Informative
{
    /// <summary>
    ///     Prints credits and the latest changelog.
    /// </summary>
    [Command("credits", Description = "Prints the credits and latest version changelog.")]
    public class CreditsCommand : ICommand
    {
        /// <inheritdoc />
        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync($@"
TML.Patcher v{GetType().Assembly.GetName().Version}
Developed by Tomat with the help of Chik3r.

Special thanks:
 - Trivaxy, for providing me with the original code for unpacking .tmod files.
 - Archanyhm, for assisting me with Linux and Mac support.
 - Chik3r, for tons of help with multithreading, unmanaged code, and more.

Release Notes - v1.0.0
 - Completely rewrote the original program.
 - Updated everything to .NET 6.0.
 - Fixed issues with ILSpy references.
 - Finally implemented various mod patching methods.
");
        }
    }
}