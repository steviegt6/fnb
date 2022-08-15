using System.IO;

namespace TML.Patcher.Extractors.Info
{
    public delegate void ReadInfoValue(BinaryReader reader, ref string tag, out string? value);
}