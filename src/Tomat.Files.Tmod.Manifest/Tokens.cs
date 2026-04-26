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

public readonly struct Token(TokenKind kind, int start, int length)
{
    public TokenKind Kind { get; } = kind;

    public int Start { get; } = start;

    public int Length { get; } = length;
}
