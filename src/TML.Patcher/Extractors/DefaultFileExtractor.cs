using TML.Files;
using TML.Files.Abstractions;

namespace TML.Patcher.Extractors
{
    public class DefaultFileExtractor : IFileExtractor
    {
        public bool ShouldExtract(ITModEntry fileEntry) {
            return true;
        }

        public IExtractedModFile Extract(ITModEntry fileEntry, byte[] data) {
            return new ExtractedModFile(fileEntry.Name, data);
        }
    }
}