using System.IO;
using Consolation.Common.Framework.OptionsSystem;

namespace TML.Patcher.Frontend.Common.Options
{
    public class ListModsOption : ConsoleOption
    {
        public override string Text => "List all located .tmod files.";

        public override void Execute()
        {
            Patcher window = Consolation.Consolation.GetWindow<Patcher>();

            window.DisplayPagedList(Program.Configuration.ItemsPerPage, Directory.GetFiles(Program.Configuration.ModsPath, "*.tmod"));
            window.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}