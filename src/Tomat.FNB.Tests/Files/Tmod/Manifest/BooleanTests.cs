using System.Linq;
using NUnit.Framework;
using Tomat.Files.Tmod.Manifest;

namespace Tomat.FNB.Tests.Files.Tmod.Manifest;

[TestFixture]
public static class BooleanTests
{
    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("TRUE", true)]
    [TestCase("false", false)]
    [TestCase("False", false)]
    [TestCase("FALSE", false)]
    public static void AcceptsTrueCaseInsensitive(string boolText, bool boolValue)
    {
        var text = $"noCompile = {boolText}";

        var result = RunBind(text);

        Assert.That(result.NoCompile, Is.EqualTo(boolValue));
    }

    [TestCase("true", false)]
    [TestCase("True", true)]
    [TestCase("TRUE", true)]
    [TestCase("false", false)]
    [TestCase("False", true)]
    [TestCase("FALSE", true)]
    public static void WarnsOnNonCanonicalCase(string boolText, bool isWarning)
    {
        var text = $"noCompile = {boolText}";

        var result = RunBind(text);

        Assert.That(result.Diagnostics.Any(x => x.Code == DiagnosticCode.NonCanonicalBoolean), Is.EqualTo(isWarning));
    }

    [Test]
    public static void RejectsInvalidBoolean()
    {
        const string text = "noCompile = maybe";

        var result = RunBind(text);

        Assert.That(result.Diagnostics.Any(x => x.Code == DiagnosticCode.InvalidBoolean), Is.True);
    }

    private static BuildProperties RunBind(string text)
    {
        var consumer = new RecordingConsumer(text);
        new BuildTxtParser(text, consumer).Parse();

        return BuildTxtBinder.Bind(text, consumer.Properties, consumer.Diagnostics);
    }
}
