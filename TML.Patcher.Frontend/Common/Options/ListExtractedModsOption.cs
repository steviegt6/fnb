using System.IO;
using Consolation.Common.Framework.OptionsSystem;

namespace TML.Patcher.Frontend.Common.Options
{
    public class ListExtractedModsOption : ConsoleOption
    {
        public override string Text => "List all folders from extracted mods.";

        public override void Execute()
        {
            Patcher window = Consolation.Consolation.GetWindow<Patcher>();

            Directory.CreateDirectory(Program.Configuration.ExtractPath);
            window.DisplayPagedList(Program.Configuration.ItemsPerPage, Directory.GetDirectories(Program.Configuration.ExtractPath));
            window.WriteOptionsList(new ConsoleOptions("Return:"));
        }
    }
}