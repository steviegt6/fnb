namespace Tomat.Files.Tmod.Manifest;

public enum ModSide : byte
{
    Both,
    Client,
    Server,
    NoSync,
}

public static class ModSideParser
{
    public static bool TryParse(
        string text,
        int start,
        int length,
        IBuildTxtConsumer consumer,
        int pos,
        out ModSide side
    )
    {
        side = ModSide.Both;

        if (length == 4 && TextUtil.EqualsIgnoreCase(text, start, "Both"))
        {
            side = ModSide.Both;
            WarnIfNonCanonical(text, start, "Both", consumer, pos);
            return true;
        }

        if (length == 6)
        {
            if (TextUtil.EqualsIgnoreCase(text, start, "Client"))
            {
                side = ModSide.Client;
                WarnIfNonCanonical(text, start, "Client", consumer, pos);
                return true;
            }

            if (TextUtil.EqualsIgnoreCase(text, start, "Server"))
            {
                side = ModSide.Server;
                WarnIfNonCanonical(text, start, "Server", consumer, pos);
                return true;
            }

            if (TextUtil.EqualsIgnoreCase(text, start, "NoSync"))
            {
                side = ModSide.NoSync;
                WarnIfNonCanonical(text, start, "NoSync", consumer, pos);
                return true;
            }
        }

        consumer.OnDiagnostic(
            new Diagnostic
            {
                Code = DiagnosticCode.InvalidModSide,
                Severity = DiagnosticSeverity.Error,
                Position = pos,
                Message = "Invalid side value",
                Hint = "Expected: Both, Client, Server, NoSync",
            }
        );

        return false;
    }

    private static void WarnIfNonCanonical(
        string text,
        int start,
        string canonical,
        IBuildTxtConsumer consumer,
        int pos
    )
    {
        if (TextUtil.EqualsExact(text, start, canonical))
        {
            return;
        }

        consumer.OnDiagnostic(
            new Diagnostic
            {
                Code = DiagnosticCode.NonCanonicalModSide,
                Severity = DiagnosticSeverity.Warning,
                Position = pos,
                Message = "Enum value casing is non-canonical",
                Hint = $"Use '{canonical}'",
            }
        );
    }
}
