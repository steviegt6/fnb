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

[Command("list", Description = "Lists files within a .tmod archive file")]
public class ListCommand : ICommand
{
    [CommandParameter(0, Description = "The .tmod archive file to unpack", IsRequired = true)]
    public string TModPath { get; set; } = string.Empty;

    public async ValueTask ExecuteAsync(IConsole console) {
        TModPath = Path.GetFullPath(TModPath);

        await console.Output.WriteLineAsync($"Listing files within \"{TModPath}\"...");

        ActionBlock<TModFileData> logBlock = new(data => console.Output.WriteLine(data.Path));

        TModFileExtractor.Extract(
            TModFileSerializer.Deserialize(TModPath),
            8,
            logBlock,
            new RawByteFileExtractor()
        );
    }
}