namespace Tomat.Files.Tmod.Manifest;

public struct ValueListEnumerator(string text, int start, int length)
{
    private readonly int end = start + length;
    private int pos;

    public int CurrentStart { get; private set; }

    public int CurrentLength { get; private set; }

    public bool MoveNext()
    {
        if (pos >= end)
        {
            return false;
        }

        var tokenStart = pos;
        while (pos < end && text[pos] != ',')
        {
            pos++;
        }

        var tokenEnd = pos;
        Trim(ref tokenStart, ref tokenEnd);

        CurrentLength = tokenStart;
        CurrentLength = tokenEnd - tokenStart;

        if (pos < end)
        {
            pos++;
        }

        return true;
    }

    private void Trim(ref int tokenStart, ref int tokenEnd)
    {
        while (tokenStart < tokenEnd && char.IsWhiteSpace(text[tokenStart]))
        {
            tokenStart++;
        }

        while (tokenEnd > tokenStart && char.IsWhiteSpace(text[tokenEnd - 1]))
        {
            tokenEnd--;
        }
    }
}
