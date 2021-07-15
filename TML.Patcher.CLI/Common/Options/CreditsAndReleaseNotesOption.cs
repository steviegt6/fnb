using Consolation.Framework.OptionsSystem;

namespace TML.Patcher.CLI.Common.Options
{
    /// <summary>
    ///     Option for viewing credits and release notes.
    /// </summary>
    public class CreditsAndReleaseNotesOption : ConsoleOption
    {
        /// <inheritdoc cref="ConsoleOption.Text"/>
        public override string Text => "View credits and release notes.";

        /// <summary>
        ///     Writes the credits and notes to the text.
        /// </summary>
        public override void Execute()
        {
            Patcher window = Program.Patcher;

            string[] contributors =
            {
                "convicted tomatophile (Stevie) - Main developer",
                "Trivaxy - Original mod unpacking code",
                "Chik3r - Improved multithreading/task code",
                "Archanyhm - Help with Linux and Mac compatibility"
            };

            string[] releaseNotes =
            {
                "Release Notes - v0.2.0.0",
                " * Added light-weight mod unpacking through drag-and-dropping.",
                " * Added the ability to add TML.Patcher.Frontend to the file context menu."
            };

            window.Clear(false);

            window.WriteLine("  Credits:");

            foreach (string contributor in contributors)
                window.WriteLine($"  {contributor}");

            window.WriteLine("--------------------");

            foreach (string note in releaseNotes)
                window.WriteLine($" {note}");

            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}