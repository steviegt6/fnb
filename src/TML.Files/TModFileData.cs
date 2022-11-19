using System.Collections.Generic;

namespace TML.Files;

public record TModFileData(string Path, byte[] Data)
{
    public string Path { get; set; } = Path;
    
    public byte[] Data { get; set; } = Data;
}