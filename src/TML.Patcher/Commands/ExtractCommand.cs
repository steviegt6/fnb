using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using TML.Files;
using TML.Files.Extraction;
using TML.Files.Extraction.Extractors;

namespace TML.Patcher.Commands;

[Command("extract", Description = "Extracts a .tmod archive file")]
public class ExtractCommand : ICommand
{
    [CommandParameter(0, Description = "The .tmod archive file to unpack", IsRequired = true)]
    public string TModPath { get; set; } = string.Empty;

    [CommandOption("output", 'o', Description = "The directory to output to")]
    public string? OutputDirectory { get; set; }

    [CommandOption("threads", 't', Description = "The amount of threads to use during extraction")]
    public int Threads { get; set; } = 8;

    public async ValueTask ExecuteAsync(IConsole console) {
        TModPath = Path.GetFullPath(TModPath);
        OutputDirectory ??= Path.GetFileNameWithoutExtension(TModPath);

        await console.Output.WriteLineAsync($"Extracting \"{TModPath}\" to \"{OutputDirectory}\"...");

        ActionBlock<TModFileData> writeBlock = new(data => {
            var path = Path.Combine(OutputDirectory, data.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
            File.WriteAllBytes(path, data.Data);
        });

        TModFileExtractor.Extract(
            TModFileSerializer.Deserialize(TModPath),
            Threads,
            writeBlock,
            new InfoFileExtractor(),
            new RawImgFileExtractor(),
            new RawByteFileExtractor()
        );
    }
}