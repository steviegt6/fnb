using NUnit.Framework;
using Tomat.Files.Tmod.Manifest;

namespace Tomat.FNB.Tests.Files.Tmod.Manifest;

[TestFixture]
public static class ValuePreservationTests
{
    [Test]
    public static void PreservesSymbolsInValue()
    {
        const string text = "displayName = A = b @ C";

        var consumer = new RecordingConsumer(text);
        new BuildTxtParser(text, consumer).Parse();

        var p = consumer.Properties[0];
        var value = text.Substring(p.ValueStart, p.ValueLength);

        Assert.That(value, Is.EqualTo("A = b @ C"));
    }

    [Test]
    public static void PreservesInternalWhitespace()
    {
        const string text = "displayName =    A   B   C   ";

        var consumer = new RecordingConsumer(text);
        new BuildTxtParser(text, consumer).Parse();

        var p = consumer.Properties[0];
        var value = text.Substring(p.ValueStart, p.ValueLength);

        Assert.That(value, Is.EqualTo("A   B   C"));
    }

    [Test]
    public static void TrimsOuterWhitespaceOnly()
    {
        const string text = "displayName =   A   ";

        var consumer = new RecordingConsumer(text);
        new BuildTxtParser(text, consumer).Parse();

        var p = consumer.Properties[0];
        var value = text.Substring(p.ValueStart, p.ValueLength);

        Assert.That(value, Is.EqualTo("A"));
    }
}
