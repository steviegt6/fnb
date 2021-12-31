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

        private static readonly string[] Contributors =
        {
            "convicted tomatophile (Stevie) - Main developer",
            "Trivaxy - Original mod unpacking code",
            "Chik3r - Improved multithreading/task code",
            "Archanyhm - Help with Linux and Mac compatibility"
        };

        private static readonly string[] ReleaseNotes =
        {
            "Release Notes - v0.2.1.0",
            " * Abolished ilspycmd in exchange for directly using the ICSharpCode.Decompiler library.",
            " * Unpacking now opens `.tmod` files with read-only perms, should help with file permission issues.",
            " * Properly support legacy `.tmod` files."
        };

        /// <summary>
        ///     Writes the credits and notes to the text.
        /// </summary>
        public override void Execute()
        {
            Patcher window = Program.Patcher;

            window.Clear(false);
            window.WriteLine("  Credits:");

            foreach (string contributor in Contributors)
                window.WriteLine($"  {contributor}");

            window.WriteLine();
            window.WriteLine("--------------------");

            foreach (string note in ReleaseNotes)
                window.WriteLine($" {note}");

            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}