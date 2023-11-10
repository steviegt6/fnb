namespace Tomat.FNB.TMOD;

public sealed record TmodFileData(string Path, byte[] Data) {
    public string Path { get; set; } = Path;

    public byte[] Data { get; set; } = Data;
}
