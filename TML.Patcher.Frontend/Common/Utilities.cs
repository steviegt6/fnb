using System;
using System.IO;

namespace TML.Patcher.Frontend.Common
{
    public static class Utilities
    {
        public static string GetModName(string path, string promptText)
        {
            Patcher window = Consolation.Consolation.GetWindow<Patcher>();

            while (true)
            {
                window.WriteAndClear(promptText, ConsoleColor.Yellow);
                string? modName = Console.ReadLine();

                if (modName == null)
                {
                    window.WriteAndClear("Specified mod name some-how returned null.");
                    continue;
                }

                if (!modName.EndsWith(".tmod"))
                    modName += ".tmod";

                if (Directory.Exists(Path.Combine(path, modName)))
                    return modName;

                window.WriteAndClear("Specified mod could not be located!");
            }
        }
    }
}