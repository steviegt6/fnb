namespace Tomat.FNB.TMOD;

public static class TmodFileHeader
{
    /// <summary>
    ///     The magic header that denotes a <c>.tmod</c> file.
    /// </summary>
    public const int TMOD_MAGIC_HEADER = 0x444F4D54; // "TMOD"

    /// <summary>
    ///     The length of the <c>.tmod</c> hash part.
    /// </summary>
    public const int HASH_LENGTH = 20;

    /// <summary>
    ///     The length of the <c>.tmod</c> signature part.
    /// </summary>
    public const int SIGNATURE_LENGTH = 256;
}