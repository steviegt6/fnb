using TML.Files;
using TML.Files.Abstractions;

namespace TML.Patcher.Extractors
{
    public class DefaultFileExtractor : IFileExtractor
    {
        public bool ShouldExtract(IModFileEntry fileEntry) {
            return true;
        }

        public IExtractedModFile Extract(IModFileEntry fileEntry, byte[] data) {
            return new ExtractedModFile(fileEntry.Name, data);
        }
    }
}