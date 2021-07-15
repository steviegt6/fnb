using Consolation.Framework.OptionsSystem;

namespace TML.Patcher.CLI.Common.Options
{
    public class CreditsAndReleaseNotesOption : ConsoleOption
    {
        public override string Text => "View credits and release notes.";

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
                "Release Notes - v0.1.3.0",
                " * Added light-weight mod unpacking through drag-and-dropping.",
                " * Added the ability to add TML.Patcher.Frontend to the file context menu."
            };

            window.Clear(false);

            window.WriteLine(1, "Credits:");
            window.SpaceCount = 2;
            foreach (string contributor in contributors)
                window.WriteLine($"{contributor}");

            window.WriteLine(0, Patcher.Line);

            foreach (string note in releaseNotes)
                window.WriteLine(1, $"{note}");

            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}