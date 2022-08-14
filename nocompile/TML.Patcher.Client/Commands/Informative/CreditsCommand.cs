using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Spectre.Console;

namespace TML.Patcher.Client.Commands.Informative
{
    /// <summary>
    ///     Prints credits and the latest changelog.
    /// </summary>
    [Command("credits", Description = "Prints the credits and latest version changelog.")]
    public class CreditsCommand : ICommand
    {
        /// <inheritdoc />
        public ValueTask ExecuteAsync(IConsole console)
        {
            AnsiConsole.MarkupLine(@"
[gray]Developed by [indianred1]Tomat[/] with the help of [white]Chik3r[/].

Special thanks:[white]
 - [yellow]Trivaxy[/], for providing me with the original code for unpacking .tmod files. While it has since been rewritten, it is what prompted me to start this project in the first place.
 - [yellow]Archanyhm[/] & [yellow]Metacinnabar[/], for assisting me with Linux and Mac support for the older implementation of ILSpy decompilation.
 - [yellow]Chik3r[/], for tons of help with multithreading, unmanaged code, decompilation, maintenance, and more.[/]

 * Release Notes *

[silver]Current - v1.0.1
 - Updated path logic to support April 2022's re-standardization of paths.
 - Added a big warning telling people to use ILSpy for decompiling.
 - Some simple error handling for end-user mistakes.

v1.0.0
 - Completely rewrote the original program.
 - Updated everything to .NET 6.0.
 - Fixed issues with ILSpy references.
 - Finally implemented various mod patching methods.[/][/]
");

            return default;
        }
    }
}