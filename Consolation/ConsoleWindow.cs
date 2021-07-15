using System;
using System.Collections.Generic;
using Consolation.Framework.OptionsSystem;

namespace Consolation
{
    /// <summary>
    ///     Base wrapper around some <see cref="Console"/> calls, as well as numerous helper-methods and <see cref="ConsoleOption"/> support.
    /// </summary>
    public abstract class ConsoleWindow
    {
        /// <summary>
        ///     The default set of <see cref="ConsoleOption"/>s. These serve as a starter as well as a fallback.
        /// </summary>
        public abstract ConsoleOptions DefaultOptions { get; }

        /// <summary>
        ///     The selected set of <see cref="ConsoleOption"/>s.
        /// </summary>
        public virtual ConsoleOptions SelectedOptions { get; set; } = null!;

        /// <summary>
        ///     Static text that should always appear at the top of the console. This is written after every clear.
        /// </summary>
        /// <param name="withMessage">Whether or not there will be an accompanying message.</param>
        public abstract void WriteStaticText(bool withMessage);

        /// <summary>
        ///     Writes an empty line to the console.
        /// </summary>
        public virtual void WriteLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        ///     Writes a line containing the object's <see cref="object.ToString"/> result.
        /// </summary>
        public virtual void WriteLine(object o)
        {
            Console.WriteLine(o);
        }

        /// <summary>
        ///     Writes the object's <see cref="object.ToString()"/> result.
        /// </summary>
        public virtual void Write(object o)
        {
            Console.Write(o);
        }

        /// <summary>
        ///     Wipes the console using <see cref="Console.Clear"/> and calls <see cref="WriteStaticText"/>.
        /// </summary>
        /// <param name="withMessage"></param>
        public virtual void Clear(bool withMessage)
        {
            Console.Clear();
            WriteStaticText(withMessage);
        }

        /// <summary>
        ///     Wipes the console using <see cref="Clear"/> and outputs the specified message in the specified color after.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public virtual void WriteAndClear(string message, ConsoleColor color = ConsoleColor.Red)
        {
            Clear(true);
            Console.ForegroundColor = color;
            WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        ///     Sets <see cref="SelectedOptions"/> to <see cref="options"/> and writes it to the console.
        /// </summary>
        /// <param name="options"></param>
        public virtual void WriteOptionsList(ConsoleOptions options)
        {
            SelectedOptions = options;
            SelectedOptions.ListForOption(this);
        }

        /// <summary>
        ///     Writes a brows-able paged list to the console.
        /// </summary>
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
                    WriteLine($" - {entryName}");
                }

                AskForInput:
                WriteLine();
                WriteLine("Goto page (-1 to exit):");

                string? input = Console.ReadLine();
                if (!int.TryParse(input, out int realInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteLine("Invalid input.");
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