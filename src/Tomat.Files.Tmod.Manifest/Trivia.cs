namespace Tomat.Files.Tmod.Manifest;

public enum TriviaKind : byte
{
    Whitespace,
    Comment,
}

public readonly struct Trivia(TriviaKind kind, int start, int length)
{
    public TriviaKind Kind { get; } = kind;

    public int Start { get; } = start;

    public int Length { get; } = length;
}
