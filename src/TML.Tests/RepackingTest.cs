using System.IO;
using NUnit.Framework;
using TML.Files;

namespace TML.Tests
{
    [TestFixture]
    public static class RepackingTest
    {
        [Test]
        public static void VerifyIdenticalBytes() {
            using var tmodFile = typeof(RepackingTest).Assembly.GetManifestResourceStream("TML.Tests.GamerMod.tmod")!;
            MemoryStream unmodified = new();
            tmodFile.CopyTo(unmodified);
            tmodFile.Position = 0;

            var file = TModFileSerializer.Deserialize(tmodFile);
            MemoryStream repacked = new();
            TModFileSerializer.Serialize(file, repacked);

            Assert.That(repacked.ToArray(), Is.EqualTo(unmodified.ToArray()));
        }
    }
}