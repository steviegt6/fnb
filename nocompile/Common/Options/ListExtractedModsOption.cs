using System.IO;
using Consolation.Framework.OptionsSystem;

namespace TML.Patcher.CLI.Common.Options
{
    /// <summary>
    ///     Lists all extracted mod folders.
    /// </summary>
    public class ListExtractedModsOption : ConsoleOption
    {
        /// <inheritdoc cref="ConsoleOption.Text"/>
        public override string Text => "List all folders from extracted mods.";

        /// <summary>
        ///     Writes a paged list of all extracted mod folders.
        /// </summary>
        public override void Execute()
        {
            Patcher window = Program.Patcher;

            Directory.CreateDirectory(Program.Configuration.ExtractPath);
            window.DisplayPagedList(Program.Configuration.ItemsPerPage, Directory.GetDirectories(Program.Configuration.ExtractPath));
            window.WriteOptionsList(new ConsoleOptions("Return:", Program.Patcher.SelectedOptions));
        }
    }
}