using System;
using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using TML.Files;
using TML.Files.Extraction;

namespace TML.Patcher.Commands;

[Command("pack", Description = "Packs a .tmod archive file")]
public class PackCommand : ICommand
{
    [CommandParameter(0, Description = "The directory to pack", IsRequired = false)]
    public string? Directory { get; set; }

    [CommandOption("output", 'o', Description = "The output file")]
    public string? OutputFile { get; set; }

    // Not 'Required' because of interactive mode.
    [CommandOption("mod-loader-version", 'v', Description = "The tModLoader version")]
    public string? ModLoaderVersion { get; set; }

    // Not 'Required' because of interactive mode.
    [CommandOption("mod-name", 'n', Description = "The mod's internal name")]
    public string? ModName { get; set; }

    [CommandOption("min-comp-size", 'c', Description = "The minimum size of a file to compress")]
    public uint MinCompSize { get; set; } = TModFile.DEFAULT_MINIMUM_COMPRESSION_SIZE;

    [CommandOption("min-comp-tradeoff", 't', Description = "The minimum compression tradeoff")]
    public float MinCompTradeoff { get; set; } = TModFile.DEFAULT_MINIMUM_COMPRESSION_TRADEOFF;

    public async ValueTask ExecuteAsync(IConsole console) {
        Directory ??= System.IO.Directory.GetCurrentDirectory();
        OutputFile ??= Path.ChangeExtension(ModName ?? new DirectoryInfo(Directory).Name, ".tmod");

        await console.Output.WriteLineAsync($"Packing \"{Directory}\" to \"{OutputFile}\"...");
        string buildTxtPath = Path.Combine(Directory, "build.txt");
        BuildProperties props;
        if (File.Exists(buildTxtPath))
            props = BuildProperties.ReadBuildInfo(File.Open(buildTxtPath, FileMode.Open, FileAccess.Read, FileShare.Read));
        else {
            await console.Output.WriteLineAsync("build.txt file not found, using default values");
            props = new BuildProperties();
        }

        TModFileSerializer.Serialize(
            TModFileExtractor.Pack(
                Directory,
                ModLoaderVersion ?? throw new ArgumentException("--mod-loader-version must be specified"),
                ModName ?? throw new ArgumentException("--mod-name must be specified"),
                props,
                MinCompSize,
                MinCompTradeoff
            ),
            OutputFile
        );
    }
}