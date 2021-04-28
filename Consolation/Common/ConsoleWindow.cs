using System;
using System.Collections.Generic;
using System.Text;
using Consolation.Common.Framework.OptionsSystem;

namespace Consolation.Common
{
    public abstract class ConsoleWindow
    {
        public abstract ConsoleOptions DefaultOptions { get; }

        public virtual int SpaceCount { get; set; }

        public abstract void WriteStaticText(bool withMessage);

        public virtual void WriteLine()
        {
            Console.WriteLine();
        }

        public virtual void WriteLine(int spaces, string message)
        {
            SpaceCount = spaces;
            WriteLine(message);
        }

        public virtual void WriteLine(string message)
        {
            StringBuilder sb = new();

            if (SpaceCount > 0)
                for (int i = 0; i < SpaceCount; i++)
                    sb.Append(' ');

            Console.WriteLine(sb.Append(message));
        }

        public virtual void Clear(bool withMessage)
        {
            Console.Clear();
            WriteStaticText(withMessage);
        }

        public virtual void WriteAndClear(string message, ConsoleColor color = ConsoleColor.Red)
        {
            SpaceCount = 0;
            Clear(true);
            Console.ForegroundColor = color;
            WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public virtual void WriteOptionsList(ConsoleOptions options)
        {
            SpaceCount = 0;
            Consolation.SelectedOptionSet = options;
            Consolation.SelectedOptionSet.ListForOption();
        }

        public virtual void DisplayPagedList<TItem>(int itemsPerPage, TItem[] items) where TItem : notnull
        {
            int totalCount = 0;
            int localCount = 0;
            List<(string, int)> localPage = new();
            List<List<(string, int)>> pages = new();

            for (int i = 0; i < items.Length; i++)
            {
                totalCount++;
                localCount++;

                localPage.Add((items[i].ToString() ?? "INVALID ENTRY", totalCount));

                if (localCount != itemsPerPage && i != items.Length - 1)
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

                WriteAndClear($"Displaying page {selectedPage + 1}/{pages.Count}.", ConsoleColor.Yellow);

                foreach ((string entryName, int entryNumber) in pages[selectedPage])
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" [{entryNumber}]");
                    Console.ForegroundColor = ConsoleColor.White;
                    WriteLine(0, $" - {entryName}");
                }

                AskForInput:
                WriteLine();
                WriteLine(0, "Goto page (-1 to exit):");
                string? input = Console.ReadLine();

                if (!int.TryParse(input, out int realInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteLine(1, "Invalid input.");
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
        }
    }
}