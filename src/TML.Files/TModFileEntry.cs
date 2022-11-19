namespace TML.Files;

public class TModFileEntry
{
    public virtual string Path { get; set; } = "";

    public virtual int Offset { get; set; }

    public virtual int Length { get; set; }

    public virtual int CompressedLength { get; set; }

    public virtual byte[]? Data { get; set; }
}