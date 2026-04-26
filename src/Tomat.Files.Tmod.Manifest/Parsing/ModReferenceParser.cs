namespace Tomat.Files.Tmod.Manifest;

public readonly struct ModReferenceSpan
{
    public int NameStart { get; init; }

    public int NameLength { get; init; }

    public int VersionStart { get; init; }

    public int VersionLength { get; init; }
}

public static class ModReferenceParser
{
    public static ModReferenceSpan Parse(string text, int start, int length)
    {
        var at = -1;

        for (var i = 0; i < length; i++)
        {
            if (text[start + i] == '@')
            {
                at = i;
                break;
            }
        }

        if (at < 0)
        {
            return new ModReferenceSpan
            {
                NameStart = start,
                NameLength = length,
            };
        }

        return new ModReferenceSpan
        {
            NameStart = start,
            NameLength = at,
            VersionStart = start + at + 1,
            VersionLength = length - at - 1,
        };
    }
}
