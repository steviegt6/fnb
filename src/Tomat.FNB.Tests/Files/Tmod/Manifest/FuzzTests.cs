using System;
using NUnit.Framework;
using Tomat.Files.Tmod.Manifest;

namespace Tomat.FNB.Tests.Files.Tmod.Manifest;

[TestFixture]
public static class FuzzTests
{
    [Test]
    public static void RandomInputDoesNotCrash()
    {
        var rand = new Random(1234);

        for (var i = 0; i < 1000; i++)
        {
            var text = RandomString(rand, 200);

            var consumer = new RecordingConsumer(text);

            new BuildTxtParser(text, consumer).Parse();

            Assert.That(consumer.Diagnostics, Has.Count.LessThan(100));
        }
    }

    private static string RandomString(Random r, int len)
    {
        var chars = new char[len];

        for (var i = 0; i < len; i++)
        {
            chars[i] = (char)r.Next(32, 126);
        }

        return new string(chars);
    }
}
