using System;
using System.IO;

namespace TML.Patcher.CLI.Common
{
    public static class Utilities
    {
        public static string GetModName(string path, string promptText, bool isDirectory = false)
        {
            while (true)
            {
                Patcher window = Program.Patcher;

                window.WriteAndClear(promptText, ConsoleColor.Yellow);
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    window.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod")) 
                    modName += ".tmod";

                if (isDirectory && Directory.Exists(Path.Combine(path, modName)))
                    return modName;

                if (File.Exists(Path.Combine(path, modName)))
                    return modName;

                window.WriteAndClear("Specified mod could not be located!");
            }
        }
    }
}