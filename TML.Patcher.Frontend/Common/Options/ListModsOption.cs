using System.IO;
using Consolation.Framework.OptionsSystem;

namespace TML.Patcher.CLI.Common.Options
{
    public class ListModsOption : ConsoleOption
    {
        public override string Text => "List all located .tmod files.";

        public override void Execute()
        {
            Patcher window = Program.Patcher;

            window.DisplayPagedList(Program.Configuration.ItemsPerPage, Directory.GetFiles(Program.Configuration.ModsPath, "*.tmod"));
            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}