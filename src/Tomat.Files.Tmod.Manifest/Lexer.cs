namespace Tomat.Files.Tmod.Manifest;

// No rolling text window support yet... sort of unnecessary for our file sizes.

internal struct Lexer(string text, IBuildTxtConsumer consumer)
{
    private int pos;
    private bool nextTokenIsValue;

    public Token NextToken()
    {
        if (nextTokenIsValue)
        {
            nextTokenIsValue = false;
            EmitInlineWhitespace();
            return ReadValueToken();
        }
        
        EmitTrivia();

        if (pos >= text.Length)
        {
            return new Token(TokenKind.EndOfFile, pos, 0);
        }

        var c = text[pos];
        if (c == '\n')
        {
            return Single(TokenKind.EndOfLine);
        }

        if (c == '=')
        {
            nextTokenIsValue = true;
            return Single(TokenKind.Equals);
        }

        if (IsIdentifierChar(c))
        {
            return ReadIdentifier();
        }

        return Single(TokenKind.Invalid);
    }

    private void EmitTrivia()
    {
        while (pos < text.Length)
        {
            var c = text[pos];

            if (c is ' ' or '\t' or '\r')
            {
                var start = pos;

                while (pos < text.Length && text[pos] is ' ' or '\t' or '\r')
                {
                    pos++;
                }

                consumer.OnTrivia(TriviaKind.Whitespace, text, start, pos - start);
                continue;
            }

            if (c == '#')
            {
                var start = pos;

                while (pos < text.Length && text[pos] != '\n')
                {
                    pos++;
                }

                consumer.OnTrivia(TriviaKind.Comment, text, start, pos - start);
                continue;
            }

            break;
        }
    }

    private void EmitInlineWhitespace()
    {
        var start = pos;

        while (pos < text.Length && text[pos] is ' ' or '\t')
        {
            pos++;
        }

        if (pos > start)
        {
            consumer.OnTrivia(TriviaKind.Whitespace, text, start, pos - start);
        }
    }

    private Token ReadValueToken()
    {
        var start = pos;
        var i = pos;

        while (i < text.Length)
        {
            var c = text[i];

            if (c is '\n' or '#')
            {
                break;
            }

            i++;
        }

        var end = i;

        // trim trailing whitespace
        // TODO: emit as trivia?
        while (end > start && text[end - 1] is ' ' or '\t')
        {
            end--;
        }

        pos = i;
        return new Token(TokenKind.Value, start, end - start);
    }

    private Token ReadIdentifier()
    {
        var start = pos;

        while (pos < text.Length && IsIdentifierChar(text[pos]))
        {
            pos++;
        }

        return new Token(TokenKind.Identifier, start, pos - start);
    }

    private static bool IsIdentifierChar(char c)
    {
        return !char.IsWhiteSpace(c) && c != '=' && c != '#';
    }

    private Token Single(TokenKind kind)
    {
        var t = new Token(kind, pos, 1);
        pos++;
        return t;
    }
}
