namespace Tomat.Files.Tmod.Manifest;

public interface IBuildTxtConsumer
{
    void OnProperty(
        PropertyKind kind,
        string text,
        int valueStart,
        int valueLength,
        int keyStart
    );

    void OnTrivia(
        TriviaKind kind,
        string text,
        int start,
        int length
    );

    void OnDiagnostic(
        Diagnostic diagnostic
    );
}
