using BenchmarkDotNet.Attributes;
using Tomat.Files.Tmod.Manifest;

namespace Tomat.FNB.Benchmarks.Files.Tmod.Manifest;

[MemoryDiagnoser]
public class BuildTxtParserBenchmarks
{
    private string text = "";

    [GlobalSetup]
    public void Setup()
    {
        text = GenerateTestFile(1000);
    }

    [Benchmark(Baseline = true)]
    public int TmlStyle()
    {
        var count = 0;

        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var idx = line.IndexOf('=');
            if (idx < 0)
            {
                continue;
            }

            var key = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim();
            _ = key;
            _ = value;

            count++;
        }

        return count;
    }

    [Benchmark]
    public int NewParser()
    {
        var consumer = new RecordingConsumer(text);
        new BuildTxtParser(text, consumer).Parse();
        return consumer.Properties.Count;
    }

    private static string GenerateTestFile(int count)
    {
        var sb = new System.Text.StringBuilder();

        for (var i = 0; i < count; i++)
        {
            sb.AppendLine($"modReferences = A@1.2.{i}, B, C");
        }

        return sb.ToString();
    }
}
