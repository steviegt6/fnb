namespace Tomat.Files.Tmod.Manifest;

public struct BuildTxtParser
{
    private readonly string text;
    private readonly IBuildTxtConsumer consumer;
    private Lexer lexer;
    private Token current;

    public BuildTxtParser(string text, IBuildTxtConsumer consumer)
    {
        this.text = text;
        this.consumer = consumer;
        lexer = new Lexer(text, consumer);
        current = lexer.NextToken();
    }

    public void Parse()
    {
        while (current.Kind != TokenKind.EndOfFile)
        {
            if (current.Kind == TokenKind.EndOfLine)
            {
                Next();
                continue;
            }

            ParseProperty();
        }
    }

    private void ParseProperty()
    {
        if (current.Kind != TokenKind.Identifier)
        {
            Error(DiagnosticCode.UnexpectedToken, "Expected property name");
            SkipLine();
            return;
        }

        var key = current;
        var kind = PropertyTable.Lookup(text, key.Start, key.Length);

        var eq = Next();
        if (eq.Kind != TokenKind.Equals)
        {
            Error(DiagnosticCode.ExpectedEquals, "Expected '=' after property name");
            SkipLine();
            return;
        }

        var value = Next();
        consumer.OnProperty(kind, text, value.Start, value.Length, key.Start);

        Next();
    }

    private void SkipLine()
    {
        while (current.Kind != TokenKind.EndOfLine && current.Kind != TokenKind.EndOfFile)
        {
            Next();
        }

        if (current.Kind == TokenKind.EndOfLine)
        {
            Next();
        }
    }

    private void Error(DiagnosticCode code, string message, string? hint = null)
    {
        consumer.OnDiagnostic(
            new Diagnostic
            {
                Code = code,
                Severity = DiagnosticSeverity.Error,
                Position = current.Start,
                Message = message,
                Hint = hint,
            }
        );
    }

    private Token Next()
    {
        return current = lexer.NextToken();
    }
}
