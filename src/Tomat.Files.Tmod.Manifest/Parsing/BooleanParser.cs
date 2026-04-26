namespace Tomat.Files.Tmod.Manifest;

public static class BooleanParser
{
    public static bool TryParse(
        string text,
        int start,
        int length,
        IBuildTxtConsumer consumer,
        int pos,
        out bool value
    )
    {
        value = false;

        if (length == 4 && TextUtil.EqualsIgnoreCase(text, start, "true"))
        {
            value = true;

            if (!TextUtil.EqualsExact(text, start, "true"))
            {
                consumer.OnDiagnostic(
                    new Diagnostic
                    {
                        Code = DiagnosticCode.NonCanonicalBoolean,
                        Severity = DiagnosticSeverity.Warning,
                        Position = pos,
                        Message = "Boolean should be lowercase",
                        Hint = "Use 'true'",
                    }
                );
            }

            return true;
        }

        if (length == 5 &&
            TextUtil.EqualsIgnoreCase(text, start, "false"))
        {
            value = false;

            if (!TextUtil.EqualsExact(text, start, "false"))
            {
                consumer.OnDiagnostic(
                    new Diagnostic
                    {
                        Code = DiagnosticCode.NonCanonicalBoolean,
                        Severity = DiagnosticSeverity.Warning,
                        Position = pos,
                        Message = "Boolean should be lowercase",
                        Hint = "Use 'false'",
                    }
                );
            }

            return true;
        }

        consumer.OnDiagnostic(
            new Diagnostic
            {
                Code = DiagnosticCode.InvalidBoolean,
                Severity = DiagnosticSeverity.Error,
                Position = pos,
                Message = "Invalid boolean value",
                Hint = "Expected 'true' or 'false'",
            }
        );

        return false;
    }
}
