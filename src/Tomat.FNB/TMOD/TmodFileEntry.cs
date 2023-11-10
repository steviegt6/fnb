namespace Tomat.FNB.TMOD;

public sealed record TmodFileEntry(string Path, int Offset, int Length, int CompressedLength, byte[]? Data) {
    public string Path { get; set; } = Path;

    public int Offset { get; set; } = Offset;

    public int Length { get; set; } = Length;

    public int CompressedLength { get; set; } = CompressedLength;

    public byte[]? Data { get; set; } = Data;
}