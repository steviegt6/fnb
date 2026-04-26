using System.Collections.Generic;

namespace Tomat.Files.Tmod.Manifest;

internal sealed class DiagnosticsForwarder(List<Diagnostic> diagnostics) : IBuildTxtConsumer
{
    public void OnProperty(PropertyKind kind, string text, int valueStart, int valueLength, int keyStart) { }

    public void OnTrivia(TriviaKind kind, string text, int start, int length) { }

    public void OnDiagnostic(Diagnostic diagnostic)
    {
        diagnostics.Add(diagnostic);
    }
}
