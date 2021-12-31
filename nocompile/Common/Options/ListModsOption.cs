using System.IO;
using Consolation.Framework.OptionsSystem;

namespace TML.Patcher.CLI.Common.Options
{
    /// <summary>
    ///     Lists all mods you can extract.
    /// </summary>
    public class ListModsOption : ConsoleOption
    {
        /// <inheritdoc cref="ConsoleOption.Text"/>
        public override string Text => "List all located .tmod files.";

        /// <summary>
        ///     Writes a paged list displaying all extractable mods.
        /// </summary>
        public override void Execute()
        {
            Patcher window = Program.Patcher;

            window.DisplayPagedList(Program.Configuration.ItemsPerPage, Directory.GetFiles(Program.Configuration.ModsPath, "*.tmod"));
            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}