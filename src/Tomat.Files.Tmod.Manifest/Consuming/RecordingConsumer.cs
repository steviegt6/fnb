using System.Collections.Generic;

namespace Tomat.Files.Tmod.Manifest;

public struct RawProperty
{
    public PropertyKind Kind { get; set; }

    public int KeyStart { get; set; }

    public int ValueStart { get; set; }

    public int ValueLength { get; set; }
}

public sealed class RecordingConsumer(string text) : IBuildTxtConsumer
{
    public List<RawProperty> Properties { get; } = [];

    public List<Diagnostic> Diagnostics { get; } = [];

    public void OnProperty(
        PropertyKind kind,
        string text,
        int valueStart,
        int valueLength,
        int keyStart
    )
    {
        Properties.Add(
            new RawProperty
            {
                Kind = kind,
                KeyStart = keyStart,
                ValueStart = valueStart,
                ValueLength = valueLength,
            }
        );
    }

    public void OnTrivia(
        TriviaKind kind,
        string text,
        int start,
        int length
    ) { }

    public void OnDiagnostic(Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }
}
