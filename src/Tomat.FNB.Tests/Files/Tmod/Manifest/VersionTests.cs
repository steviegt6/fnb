using System.Linq;
using NUnit.Framework;
using Tomat.Files.Tmod.Manifest;

namespace Tomat.FNB.Tests.Files.Tmod.Manifest;

[TestFixture]
public static class VersionTests
{
    [Test]
    public static void ParsesValidVersion()
    {
        const string text = "version = 1.2.3";

        var result = Run(text);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Version.Major, Is.EqualTo(1));
            Assert.That(result.Version.Minor, Is.EqualTo(2));
            Assert.That(result.Version.Build, Is.EqualTo(3));
        }
    }

    [Test]
    public static void RejectsInvalidVersion()
    {
        const string text = "version = 1.a.3";

        var result = Run(text);

        Assert.That(result.Diagnostics.Any(x => x.Code == DiagnosticCode.InvalidVersion), Is.True);
    }

    private static BuildProperties Run(string text)
    {
        var c = new RecordingConsumer(text);
        new BuildTxtParser(text, c).Parse();
        return BuildTxtBinder.Bind(text, c.Properties, c.Diagnostics);
    }
}
