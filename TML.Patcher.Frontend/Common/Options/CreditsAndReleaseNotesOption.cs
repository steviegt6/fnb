using System;
using System.IO;
using Consolation.Common.Framework.OptionsSystem;
using Newtonsoft.Json;

namespace TML.Patcher.Frontend.Common.Options
{
    public class CreditsAndReleaseNotesOption : ConsoleOption
    {
        public override string Text => "View credits and release notes.";

        public override void Execute()
        {
            /*
            WriteLine(1, "Credits:");
            SpaceCount = 2;
            foreach (string contributor in contributors)
                WriteLine($"{contributor}");
            WriteLine(0, Line);
            foreach (string note in releaseNotes)
                WriteLine(1, $"{note}");
             */
            Patcher window = Consolation.Consolation.GetWindow<Patcher>();

            string[] contributors =
            {
                "convicted tomatophile (Stevie) - Main developer",
                "Trivaxy - Original mod unpacking code",
                "Chik3r - Improved multithreading/task code",
                "Archanyhm - Help with Linux and Mac compatibility"
            };

            string[] releaseNotes =
            {
                "Release Notes - v0.1.2.0",
                " * Added release notes.",
                " * Added configurable progress bar.",
                " * Official splitting of the frontend and backend.",
                " * Internal code clean-up.",
                " * Added One Drive directory detection.",
                " * Added configurable page count.",
                " * Moved release notes and credits to their own page."
            };

            window.Clear(false);

            window.WriteLine(1, "Credits:");
            window.SpaceCount = 2;
            foreach (string contributor in contributors)
                window.WriteLine($"{contributor}");

            window.WriteLine(0, Patcher.Line);

            foreach (string note in releaseNotes)
                window.WriteLine(1, $"{note}");

            window.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}