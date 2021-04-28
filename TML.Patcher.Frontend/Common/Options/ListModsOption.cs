using System;
using System.Collections.Generic;
using System.IO;
using Consolation;
using Consolation.Common.Framework.OptionsSystem;

namespace TML.Patcher.Frontend.Common.Options
{
    public class ListModsOption : ConsoleOption
    {
        public override string Text => "List all located .tmod files.";

        public override void Execute()
        {
            Patcher window = ConsoleAPI.GetWindow<Patcher>();

            int modCount = 0;
            int localCount = 0;
            List<(string, int)> localPage = new();
            List<List<(string, int)>> pages = new();
            string[] files = Directory.GetFiles(Program.Configuration.ModsPath, "*.tmod");
            for (int i = 0; i < files.Length; i++)
            {
                modCount++;
                localCount++;
                localPage.Add((files[i], modCount));

                if (localCount != 10 && i != files.Length - 1)
                    continue;

                pages.Add(localPage);
                localPage = new List<(string, int)>();
                localCount = 0;
            }

            int selectedPage = 0;
            while (true)
            {
                if (selectedPage >= pages.Count)
                    break;

                window.WriteAndClear($"Displaying page {selectedPage + 1}/{pages.Count}.", ConsoleColor.Yellow);
                foreach ((string modName, int modNumber) in pages[selectedPage])
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" [{modNumber}]");
                    Console.ForegroundColor = ConsoleColor.White;
                    window.WriteLine(0, $" - {modName}");
                }

                AskForInput:
                window.WriteLine();
                window.WriteLine(0, "Goto page (-1 to exit):");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out int realInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    window.WriteLine(1, " Invalid input.");
                    Console.ForegroundColor = ConsoleColor.White;
                    goto AskForInput;
                }

                if (realInput <= -1)
                    break;

                if (realInput > pages.Count)
                    realInput = pages.Count;

                if (realInput == 0)
                    realInput = 1;

                selectedPage = realInput - 1;
            }

            window.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}
