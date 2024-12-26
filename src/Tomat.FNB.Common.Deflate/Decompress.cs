namespace Tomat.FNB.Common.Deflate;

public static class Decompress
{
    public const uint LITERAL_ENTRY         = 0x8000;
    public const uint EXCEPTIONAL_ENTRY     = 0x4000;
    public const uint SECONDARY_TABLE_ENTRY = 0x2000;
}