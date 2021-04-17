using System;
using System.IO;
using TML.Patcher.Common.Framework;

namespace TML.Patcher.Common.Options
{
    // TODO: pages
    public class ListModsOption : ConsoleOption
    {
        public override string Text => "List all located .tmod files.";

        public override void Execute()
        {
            int modCount = 0;

            foreach (string file in Directory.GetFiles(Program.Configuration.ModsPath, "*.tmod"))
            {
                modCount++;
                Console.Write($"  [{modCount}] ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{file}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            Program.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}
