namespace Tomat.Files.Tmod.Manifest;

public enum TriviaKind : byte
{
    Whitespace,
    Comment,
}

public readonly record struct Trivia(
    TriviaKind Kind,
    int Start,
    int Length
);
