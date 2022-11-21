using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TML.Files;
using TML.Files.Extraction;
using TML.Files.Extraction.Extractors;

namespace TML.Tests
{
    [TestFixture]
    public static class ExtractingTest
    {
        private static readonly string[] Files =
        {
            "GamerMod.pdb",
            "GamerMod.dll",
            "build.txt",
            // "Info",
            "Properties/launchSettings.json",
            "description.txt",
            "icon.png",
            "workshop.json",
            "Gores/GoreTest.png",
            // "Gores/GoreTest.rawimg",
            "GamerMod.zip"
        };

        [Test]
        public static void ContainsExactlyTheExpectedFiles() {
            using var tmodFile = typeof(ExtractingTest).Assembly.GetManifestResourceStream("TML.Tests.GamerMod.tmod")!;
            var file = TModFileSerializer.Deserialize(tmodFile);
            IFileExtractor[] extractors = {new InfoFileExtractor(), new RawImgFileExtractor(), new RawByteFileExtractor()};

            IEnumerable<string> extractedFiles = TModFileExtractor.Extract(file, 8, extractors).Select(x => x.Path);
            CollectionAssert.AreEquivalent(extractedFiles, Files);
        }

        [Test]
        // This is not a unit test.
        public static void ThisTestSucks() {
            using var tmodFile = typeof(ExtractingTest).Assembly.GetManifestResourceStream("TML.Tests.GamerMod.tmod")!;
            var file = TModFileSerializer.Deserialize(tmodFile);
            IFileExtractor[] extractors = {new InfoFileExtractor(), new RawImgFileExtractor(), new RawByteFileExtractor()};

            if (Directory.Exists("GamerMod")) Directory.Delete("GamerMod", true);

            IEnumerable<TModFileData> extractedFiles = TModFileExtractor.Extract(file, 8, extractors);
            foreach (var extractedFile in extractedFiles) {
                string path = Path.Combine("GamerMod", extractedFile.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, extractedFile.Data);
            }
        }
    }
}