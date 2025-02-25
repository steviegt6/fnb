using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

using JetBrains.Annotations;

using Tomat.FNB.Common.IO;
using Tomat.FNB.TMOD;
using Tomat.FNB.TMOD.Converters;
using Tomat.FNB.TMOD.Converters.Extractors;

namespace Tomat.FNB.CLI.Commands.TMOD.Abstract;

/// <summary>
///     An abstracted command providing basic expected options and parameters
///     for interacting with a <c>.tmod</c> file which should be extracted.
/// </summary>
public abstract class AbstractTmodExtractCommand : ICommand
{
#region Options
    [CommandOption(
        "output-path",
        'o',
        Description = "The path to extract the .tmod to.  If not specified, it will be extracted to `./<mod-name>.`",
        IsRequired = false
    )]
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public string? OutputPath { get; set; }

    [CommandOption(
        "list-files",
        'l',
        Description = "Whether to just output all the file names found in the .tmod instead of extracting any files.",
        IsRequired = false
    )]
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public bool ListFileNames { get; set; }

    [CommandOption(
        "sort-file-names",
        's',
        Description = "When paired with `--list-files`, guarantees file names will be sorted alphabetically; ignored otherwise.",
        IsRequired = false
    )]
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public bool SortFileNames { get; set; }

    [CommandOption(
        "pure",
        'p',
        Description = "When extracting files, whether to not perform known file conversions to better representations (i.e. no .rawimg -> .png, Info -> build.txt); ignored otherwise.",
        IsRequired = false
    )]
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public bool PureFiles { get; set; }
#endregion

    public abstract ValueTask ExecuteAsync(IConsole console);

    /// <summary>
    ///     Extracts a .tmod archive.
    /// </summary>
    protected async ValueTask ExtractArchive(
        IConsole console,
        string   archivePath,
        string?  destinationPath
    )
    {
        if (!File.Exists(archivePath))
        {
            throw new FileNotFoundException($"Could not find .tmod file: {archivePath}");
        }

        var tmodFile = CommandUtil.ReadTmodFile(archivePath);
        if (tmodFile is null)
        {
            throw new InvalidOperationException($"Failed to read the file, are you sure it's a .tmod archive?: {archivePath}");
        }

        if (ListFileNames)
        {
            var fileNames = tmodFile.FileNames;
            if (SortFileNames)
            {
                fileNames = fileNames.OrderBy(x => x).ToList();
            }

            foreach (var fileName in fileNames)
            {
                await console.Output.WriteLineAsync(fileName);
            }
            return;
        }

        var converters = PureFiles ? Array.Empty<IFileConverter>() : [new InfoExtractor(), RawImgExtractor.GetInstance()];
        await CommandUtil.ExtractArchive(console, tmodFile, archivePath, destinationPath, converters);
    }
}