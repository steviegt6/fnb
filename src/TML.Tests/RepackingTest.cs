using System.IO;
using NUnit.Framework;
using TML.Files;
using TML.Files.Abstractions;

namespace TML.Tests
{
    public class RepackingTest
    {
        [Test]
        public static void VerifyIdenticalBytes() {
            using Stream tmodFile = typeof(RepackingTest).Assembly.GetManifestResourceStream("TML.Tests.GamerMod.tmod")!;
            MemoryStream unmodified = new();
            tmodFile.CopyTo(unmodified);
            tmodFile.Position = 0;
            
            IModFileReader reader = new ModFileReader();
            IModFileWriter<ModFileWriterSettings> writer = new ModFileWriter();
            IModFile file = reader.Read(tmodFile);
            MemoryStream repacked = new();
            writer.Write(file, repacked, new ModFileWriterSettings(file.Header, file.ModLoaderVersion, file.Name, file.Version));

            Assert.That(repacked.ToArray(), Is.EqualTo(unmodified.ToArray()));
        }
    }
}