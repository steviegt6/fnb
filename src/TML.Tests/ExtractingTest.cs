using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TML.Files;
using TML.Files.Abstractions;
using TML.Patcher.Extractors;
using TML.Patcher.Extractors.Info;

namespace TML.Tests
{
    public class ExtractingTest
    {
        private static readonly string[] Files =
        {
            "GamerMod.pdb",
            "GamerMod.dll",
            "build.txt",
            "Info",
            "Properties/launchSettings.json",
            "description.txt",
            "icon.png",
            "workshop.json",
            "Gores/GoreTest.png",
            "Gores/GoreTest.rawimg",
            "GamerMod.zip"
        };
        
        [Test]
        public static void ContainsExactlyTheExpectedFiles() {
            using Stream tmodFile = typeof(ExtractingTest).Assembly.GetManifestResourceStream("TML.Tests.GamerMod.tmod")!;
            IModFileReader reader = new ModFileReader();
            IModFileExtractor extractor = new ModFileExtractor();
            IModFile file = reader.Read(tmodFile);
            IFileExtractor[] extractors = {new InfoFileExtractor(), new RawImgFileExtractor(), new DefaultFileExtractor()};
            
            IEnumerable<string> extractedFiles = extractor.Extract(file, extractors).Select(x => x.LocalPath);
            CollectionAssert.AreEquivalent(extractedFiles, Files);
        }

        [Test]
        // This is not a unit test.
        public static void ThisTestSucks() {
            using Stream tmodFile = typeof(ExtractingTest).Assembly.GetManifestResourceStream("TML.Tests.GamerMod.tmod")!;
            IModFileReader reader = new ModFileReader();
            IModFileExtractor extractor = new ModFileExtractor();
            IModFile file = reader.Read(tmodFile);
            IFileExtractor[] extractors = {new InfoFileExtractor(), new RawImgFileExtractor(), new DefaultFileExtractor()};

            Directory.Delete("GamerMod", true);
            
            IEnumerable<IExtractedModFile> extractedFiles = extractor.Extract(file, extractors);
            foreach (IExtractedModFile extractedFile in extractedFiles) {
                string path = Path.Combine("GamerMod", extractedFile.LocalPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, extractedFile.Data);
            }
        }
    }
}