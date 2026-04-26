namespace Tomat.Files.Tmod.Manifest;

internal static class TextUtil
{
    public static bool EqualsIgnoreCase(string text, int start, string keyword)
    {
        if (start + keyword.Length > text.Length)
        {
            return false;
        }

        for (var i = 0; i < keyword.Length; i++)
        {
            var a = text[start + i];
            var b = keyword[i];

            if (ToLowerAscii(a) != ToLowerAscii(b))
            {
                return false;
            }
        }

        return true;
    }

    public static bool EqualsExact(string text, int start, string keyword)
    {
        for (var i = 0; i < keyword.Length; i++)
        {
            if (text[start + i] != keyword[i])
            {
                return false;
            }
        }

        return true;
    }

    private static char ToLowerAscii(char c)
    {
        if (c is >= 'A' and <= 'Z')
        {
            return (char)(c + 32);
        }

        return c;
    }
}
