using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
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

            List<string> paths = new();
            TModFileExtractor.Extract(file, 8, new ActionBlock<TModFileData>(data => paths.Add(data.Path)), extractors);
            CollectionAssert.AreEquivalent(paths, Files);
        }

        [Test]
        // This is not a unit test.
        public static void ThisTestSucks() {
            using var tmodFile = typeof(ExtractingTest).Assembly.GetManifestResourceStream("TML.Tests.GamerMod.tmod")!;
            var file = TModFileSerializer.Deserialize(tmodFile);
            IFileExtractor[] extractors = {new InfoFileExtractor(), new RawImgFileExtractor(), new RawByteFileExtractor()};

            if (Directory.Exists("GamerMod")) Directory.Delete("GamerMod", true);

            ActionBlock<TModFileData> writeBlock = new(data => {
                string path = Path.Combine("GamerMod", data.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
                File.WriteAllBytes(path, data.Data);
            });
            
            TModFileExtractor.Extract(file, 8, writeBlock, extractors);
        }
    }
}