using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using Tomat.FNB.TMOD;

namespace Tomat.FNB.Commands.TMOD;

public abstract class TmodAbstractExtractCommand : ICommand {
    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("out-dir", 'o', Description = "The directory to extract the mod to. If not specified, it will be extracted to ./<mod name>", IsRequired = false)]
    public string? OutputDirectory { get; set; }

    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("file", 'f', Description = "The file to extract from the .tmod archive, if a single one is requested", IsRequired = false)]
    public string? File { get; set; }

    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("list", 'l', Description = "List the files in the .tmod archive instead of extracting", IsRequired = false)]
    public bool ListFiles { get; set; }

    [UsedImplicitly(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign)]
    [CommandOption("pure", 'p', Description = "Whether to use pure file representations (no conversions, i.e. no .rawimg -> .png)", IsRequired = false)]
    public bool Pure { get; set; }

    public abstract ValueTask ExecuteAsync(IConsole console);

    protected async ValueTask ExtractArchive(IConsole console, string archivePath, string? destinationPath) {
        if (File is null && !ListFiles) {
            await CommandUtil.ExtractArchive(console, archivePath, destinationPath);
            return;
        }

        if (ListFiles) {
            if (!TmodFile.TryReadFromPath(archivePath, out var tmodFile)) {
                await console.Error.WriteLineAsync($"Failed to read \"{archivePath}\".");
                return;
            }

            if (Pure) {
                await console.Output.WriteLineAsync($"Files in \"{archivePath}\":");
                foreach (var entry in tmodFile.Entries)
                    await console.Output.WriteLineAsync(entry.Path);
            }
            else {
                ActionBlock<TmodFileData> finalBlock = new(
                    async data => {
                        await console.Output.WriteLineAsync(data.Path);
                    },
                    new ExecutionDataflowBlockOptions {
                        MaxDegreeOfParallelism = 1,
                    }
                );

                await console.Output.WriteLineAsync($"Files in \"{archivePath}\":");
                tmodFile.Extract(finalBlock);
            }

            return;
        }

        if (File is not null) {
            destinationPath ??= Path.GetFileNameWithoutExtension(archivePath);
            destinationPath = Path.Combine(destinationPath, File);

            if (System.IO.File.Exists(destinationPath))
                System.IO.File.Delete(destinationPath);

            var dir = Path.GetDirectoryName(destinationPath);
            if (dir is not null)
                Directory.CreateDirectory(dir);

            if (!TmodFile.TryReadFromPath(archivePath, out var tmodFile)) {
                await console.Error.WriteLineAsync($"Failed to read \"{archivePath}\".");
                return;
            }

            if (Pure) {
                var entry = tmodFile.Entries.Find(e => e.Path == File);
                if (entry.Data is null) {
                    await console.Error.WriteLineAsync($"No file found in \"{archivePath}\" with the name \"{File}\".");
                    return;
                }

                await console.Output.WriteLineAsync($"Extracting \"{File}\" from \"{archivePath}\" to \"{destinationPath}\"...");

                // await System.IO.File.WriteAllBytesAsync(destinationPath, entry.Data.Array);
                await using var fs = System.IO.File.Open(destinationPath, FileMode.OpenOrCreate, FileAccess.Write); 
                fs.Write(entry.Data.Span);
            }
            else {
                var found = false;

                ActionBlock<TmodFileData> finalBlock = new(
                    async data => {
                        if (data.Path == File) {
                            found = true;
                            await console.Output.WriteLineAsync($"Extracting \"{File}\" from \"{archivePath}\" to \"{destinationPath}\"...");
                            
                            // await System.IO.File.WriteAllBytesAsync(destinationPath, data.Data.Array);
                            await using var fs = System.IO.File.Open(destinationPath, FileMode.OpenOrCreate, FileAccess.Write); 
                            fs.Write(data.Data.Span);
                        }
                    },
                    new ExecutionDataflowBlockOptions {
                        MaxDegreeOfParallelism = 1,
                    }
                );

                tmodFile.Extract(finalBlock);

                if (!found) {
                    await console.Error.WriteLineAsync($"No file found in \"{archivePath}\" with the name \"{File}\".");
                    return;
                }
            }

            return;
        }

        throw new Exception("Impossible state reached");
    }
}
