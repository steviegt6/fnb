using System.IO;
using Consolation.Framework.OptionsSystem;

namespace TML.Patcher.CLI.Common.Options
{
    public class ListExtractedModsOption : ConsoleOption
    {
        public override string Text => "List all folders from extracted mods.";

        public override void Execute()
        {
            Patcher window = Program.Patcher;

            Directory.CreateDirectory(Program.Configuration.ExtractPath);
            window.DisplayPagedList(Program.Configuration.ItemsPerPage, Directory.GetDirectories(Program.Configuration.ExtractPath));
            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}