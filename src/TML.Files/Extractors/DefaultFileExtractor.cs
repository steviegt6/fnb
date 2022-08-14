using TML.Files.Abstractions;

namespace TML.Files.Extractors
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