namespace Tomat.FNB.TMOD;

public readonly record struct TmodFileEntry(string Path, int Offset, int Length, int CompressedLength, byte[]? Data);
