namespace Tomat.Files.Tmod.Manifest;

public enum TokenKind : byte
{
    // <key> = value
    Identifier,

    // =
    Equals,

    // key = <value>
    Value,

    // EOL
    EndOfLine,

    // EOF
    EndOfFile,

    // anything else
    Invalid,
}

public readonly record struct Token(
    TokenKind Kind,
    int Start,
    int Length
);
