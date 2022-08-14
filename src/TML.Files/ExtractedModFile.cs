using TML.Files.Abstractions;

namespace TML.Files
{
    /// <summary>
    ///     Default, tModLoader-compliant <see cref="IExtractedModFile"/> implementation.
    /// </summary>
    public class ExtractedModFile : IExtractedModFile
    {
        public string LocalPath { get; }
        
        public byte[] Data { get; }
        
        public ExtractedModFile(string localPath, byte[] data) {
            LocalPath = localPath;
            Data = data;
        }
    }
}