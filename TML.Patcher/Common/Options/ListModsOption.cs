using System;
using System.Collections.Generic;
using System.IO;
using Consolation.Common.Framework.OptionsSystem;

namespace TML.Patcher.Common.Options
{
    public class ListModsOption : ConsoleOption
    {
        public override string Text => "List all located .tmod files.";

        public override void Execute()
        {
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

                Program.Instance.WriteAndClear($"Displaying page {selectedPage + 1}/{pages.Count}.", ConsoleColor.Yellow);
                foreach ((string modName, int modNumber) in pages[selectedPage])
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" [{modNumber}]");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($" - {modName}");
                }

                AskForInput:
                Console.WriteLine();
                Console.WriteLine("Goto page (-1 to exit):");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out int realInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Invalid input.");
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

            Program.Instance.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}
