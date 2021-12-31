using System;

namespace TML.Patcher.CLI.Utilities
{
    /// <summary>
    ///     Reusable utilities for handling text in the console.
    /// </summary>
    public static class ConsoleUtilities
    {
        public static void ClearAboveLines(int count)
        {
            ClearLine(); // flush cur. line
            
            for (int _ = 0; _ < count; _++)
            {
                Console.CursorTop--;
                ClearLine();
            }
        }
        
        public static void ClearLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        /// <summary>
        ///     Set the background to white and foreground to black. Can customize individually.
        /// </summary>
        public static void ColorInvert(
            ConsoleColor background = ConsoleColor.White,
            ConsoleColor foreground = ConsoleColor.Black
        )
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
        }

        public static void ColorReset() => Console.ResetColor();

        public static void WriteMany(string text, int count)
        {
            for (int _ = 0; _ < count; _++)
                Console.Write(text);
        }
    }
}