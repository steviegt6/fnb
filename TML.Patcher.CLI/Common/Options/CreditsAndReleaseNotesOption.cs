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

        public static readonly string[] ReleaseNotes =
        {
            "Release Notes - v0.2.0.0",
            " * Behind-the-scenes code refactorization.",
            " * Consolation is completely independent.",
            " * TMl.Patcher now uses DragonFruit.",
            " * Backend renamed to TML.Patcher.",
            " * Frontend renamed to TML.Patcher.CLI.",
            " * Uploaded TML.Files and TML.Patcher to NuGet.",
            " * Repackaging mods is now possible."
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