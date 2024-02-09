namespace Tomat.FNB.XNB.LZX;

internal static class LzxConstants {
    public const ushort MIN_MATCH = 2;
    public const ushort MAX_MATCH = 257;
    public const ushort NUM_CHARS = 256;

    public enum BlockType {
        Invalid      = 0,
        Verbatim     = 1,
        Aligned      = 2,
        Uncompressed = 3,
    }

    public const ushort PRETREE_NUM_ELEMENTS = 20;
    public const ushort ALIGNED_NUM_ELEMENTS = 8;
    public const ushort NUM_PRIMARY_LENGTHS = 7;
    public const ushort NUM_SECONDARY_LENGTHS = 249;

    public const ushort PRETREE_MAX_SYMBOLS = PRETREE_NUM_ELEMENTS;
    public const ushort PRETREE_TABLE_BITS = 6;
    public const ushort MAINTREE_MAX_SYMBOLS = NUM_CHARS + 50 * 8;
    public const ushort MAINTREE_TABLE_BITS = 12;
    public const ushort LENGTH_MAX_SYMBOLS = NUM_SECONDARY_LENGTHS + 1;
    public const ushort LENGTH_TABLE_BITS = 12;
    public const ushort ALIGNED_MAX_SYMBOLS = ALIGNED_NUM_ELEMENTS;
    public const ushort ALIGNED_TABLE_BITS = 7;

    public const ushort LEN_TABLE_SAFETY =  64;
}
