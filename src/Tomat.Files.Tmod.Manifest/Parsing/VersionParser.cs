namespace Tomat.Files.Tmod.Manifest;

public struct ParsedVersion
{
    public int Major { get; set; }

    public int Minor { get; set; }

    public int Patch { get; set; }

    public int Revision { get; set; }

    public bool HasMinor => Minor > -1;

    public bool HasPatch => Patch > -1;

    public bool HasRevision => Revision > -1;
}

public static class VersionParser
{
    public static bool TryParse(
        string text,
        int start,
        int length,
        out ParsedVersion version,
        IBuildTxtConsumer consumer,
        int errorPos
    )
    {
        version = new ParsedVersion
        {
            Major = 1,
            Minor = -1,
            Patch = -1,
            Revision = -1,
        };

        var part = 0;
        var value = 0;

        for (var i = 0; i <= length; i++)
        {
            var end = i == length;
            var c = end ? '.' : text[start + i];

            if (c == '.')
            {
                if (!Commit(ref version, part, value, consumer, errorPos))
                {
                    return false;
                }

                part++;
                value = 0;
                continue;
            }

            if (!char.IsDigit(c))
            {
                consumer.OnDiagnostic(
                    new Diagnostic
                    {
                        Code = DiagnosticCode.InvalidVersion,
                        Severity = DiagnosticSeverity.Error,
                        Position = errorPos,
                        Message = "Invalid verison",
                        Hint = "Use only digits and dots",
                    }
                );
                return false;
            }

            value = value * 10 + (c - '0');
        }

        return true;
    }

    private static bool Commit(
        ref ParsedVersion v,
        int part,
        int value,
        IBuildTxtConsumer consumer,
        int pos
    )
    {
        switch (part)
        {
            case 0:
                v.Major = value;
                return true;

            case 1:
                v.Minor = value;
                return true;

            case 2:
                v.Patch = value;
                return true;

            case 3:
                v.Revision = value;
                return true;

            default:
                consumer.OnDiagnostic(
                    new Diagnostic
                    {
                        Code = DiagnosticCode.TooManyVersionParts,
                        Severity = DiagnosticSeverity.Error,
                        Position = pos,
                        Message = "Too many version components",
                        Hint = "Max is 4 parts (a.b.c.d)",
                    }
                );
                return false;
        }
    }
}
