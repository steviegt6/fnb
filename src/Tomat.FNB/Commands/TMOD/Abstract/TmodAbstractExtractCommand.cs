using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

using JetBrains.Annotations;

using Tomat.FNB.TMOD;
using Tomat.FNB.TMOD.Converters.Extractors;
using Tomat.FNB.TMOD.Utilities;

namespace Tomat.FNB.Commands.TMOD.Abstract;

public abstract class TmodAbstractExtractCommand : ICommand
{
#region Options
    /// <summary>
    ///     The directory to extract the mod to. If not specified, it will be
    ///     extracted to <c>./&lt;mod name&gt;</c>.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("out-dir", 'o', Description = "The directory to extract the mod to. If not specified, it will be extracted to ./<mod name>", IsRequired = false)]
    public string? OutDir { get; set; }

    /// <summary>
    ///     The file to extract from the .tmod archive, if a single file is
    ///     requested.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("file", 'f', Description = "The file to extract from the .tmod archive, if a single file is requested", IsRequired = false)]
    public string? File { get; set; }

    /// <summary>
    ///     List the files in the .tmod archive instead of extracting.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("list", 'l', Description = "List the files in the .tmod archive instead of extracting", IsRequired = false)]
    public bool List { get; set; }

    /// <summary>
    ///     Whether to use pure file representations (no conversions, i.e. no
    ///     <c>.rawimg</c> -> <c>.png</c>).
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("pure", 'p', Description = "Whether to use pure file representations (no conversions, i.e. no .rawimg -> .png)", IsRequired = false)]
    public bool Pure { get; set; }
#endregion

    public abstract ValueTask ExecuteAsync(IConsole console);

    protected async ValueTask ExtractArchive(IConsole console, string archivePath, string? destinationPath)
    {
        if (File is null && !List)
        {
            await CommandUtil.ExtractArchive(console, archivePath, destinationPath);
            return;
        }

        if (List)
        {
            IReadOnlyTmodFile tmodFile;
            try
            {
                await using var fs = System.IO.File.OpenRead(archivePath);
                {
                    var serializableTmodFile = SerializableTmodFile.FromStream(fs);
                    tmodFile = serializableTmodFile.Convert(Pure ? [] : [RawimgExtractor.GetRawimgExtractor(), new InfoExtractor()]);
                }
            }
            catch (Exception e)
            {
                await console.Error.WriteLineAsync($"Failed to read \"{archivePath}\": {e}");
                return;
            }

            await console.Output.WriteLineAsync($"Files in \"{archivePath}\":");
            foreach (var (path, _) in tmodFile.Entries)
            {
                await console.Output.WriteLineAsync(path);
            }

            return;
        }

        if (File is not null)
        {
            destinationPath ??= Path.GetFileNameWithoutExtension(archivePath);
            destinationPath =   Path.Combine(destinationPath, File);

            if (System.IO.File.Exists(destinationPath))
                System.IO.File.Delete(destinationPath);

            var dir = Path.GetDirectoryName(destinationPath);
            if (dir is not null)
                Directory.CreateDirectory(dir);

            IReadOnlyTmodFile tmodFile;
            try
            {
                await using var fs = System.IO.File.OpenRead(archivePath);
                {
                    var serializableTmodFile = SerializableTmodFile.FromStream(fs);
                    tmodFile = serializableTmodFile.Convert(Pure ? [] : [RawimgExtractor.GetRawimgExtractor(), new InfoExtractor()]);
                }
            }
            catch (Exception e)
            {
                await console.Error.WriteLineAsync($"Failed to read \"{archivePath}\": {e}");
                return;
            }

            if (!tmodFile.Entries.TryGetValue(File, out var entry))
            {
                await console.Error.WriteLineAsync($"No file found in \"{archivePath}\" with the name \"{File}\".");
                return;
            }

            await console.Output.WriteLineAsync($"Extracting \"{File}\" from \"{archivePath}\" to \"{destinationPath}\"...");

            await System.IO.File.WriteAllBytesAsync(destinationPath, entry);
            return;
        }

        throw new Exception("Impossible state reached");
    }
}