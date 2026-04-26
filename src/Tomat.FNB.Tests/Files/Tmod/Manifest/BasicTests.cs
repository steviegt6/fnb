using NUnit.Framework;
using Tomat.Files.Tmod.Manifest;

namespace Tomat.FNB.Tests.Files.Tmod.Manifest;

[TestFixture]
public static class BasicTests
{
    [Test]
    public static void ParsesSimpleProperty()
    {
        const string text = "author = test";

        var consumer = new RecordingConsumer(text);
        new BuildTxtParser(text, consumer).Parse();

        Assert.That(consumer.Properties, Has.Count.EqualTo(1));

        var p = consumer.Properties[0];
        Assert.That(p.Kind, Is.EqualTo(PropertyKind.Author));

        var value = text.Substring(p.ValueStart, p.ValueLength);
        Assert.That(value, Is.EqualTo("test"));
    }

    [Test]
    public static void IgnoresEmptyLines()
    {
        const string text = "\n\nauthor = a\n\n";

        var consumer = new RecordingConsumer(text);
        new BuildTxtParser(text, consumer).Parse();

        Assert.That(consumer.Properties, Has.Count.EqualTo(1));
    }
}
