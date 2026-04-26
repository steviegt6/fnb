using System.Linq;
using NUnit.Framework;
using Tomat.Files.Tmod.Manifest;

namespace Tomat.FNB.Tests.Files.Tmod.Manifest;

[TestFixture]
public static class ModSideTests
{
    [TestCase("client", ModSide.Client)]
    [TestCase("Client", ModSide.Client)]
    [TestCase("CLIENT", ModSide.Client)]
    [TestCase("server", ModSide.Server)]
    [TestCase("Server", ModSide.Server)]
    [TestCase("SERVER", ModSide.Server)]
    [TestCase("nosync", ModSide.NoSync)]
    [TestCase("noSync", ModSide.NoSync)]
    [TestCase("NoSync", ModSide.NoSync)]
    [TestCase("NOSYNC", ModSide.NoSync)]
    [TestCase("both", ModSide.Both)]
    [TestCase("Both", ModSide.Both)]
    [TestCase("BOTH", ModSide.Both)]
    public static void AcceptsCaseInsensitiveEnum(string value, ModSide modSide)
    {
        var text = $"side = {value}";
        var result = Run(text);
        
        Assert.That(result.Side, Is.EqualTo(modSide));
    }

    [TestCase("client", true)]
    [TestCase("Client", false)]
    [TestCase("CLIENT", true)]
    [TestCase("server", true)]
    [TestCase("Server", false)]
    [TestCase("SERVER", true)]
    [TestCase("nosync", true)]
    [TestCase("noSync", true)]
    [TestCase("NoSync", false)]
    [TestCase("NOSYNC", true)]
    [TestCase("both", true)]
    [TestCase("Both", false)]
    [TestCase("BOTH", true)]
    public static void WarnsOnCaseMismatch(string value, bool isWarning)
    {
        var text = $"side = {value}";
        
        var result = Run(text);
        
        Assert.That(result.Diagnostics.Any(x => x.Code == DiagnosticCode.NonCanonicalModSide), Is.EqualTo(isWarning));
    }

    [Test]
    public static void RejectsInvalidEnum()
    {
        const string text = "side = none";
        
        var result = Run(text);
        
        Assert.That(result.Diagnostics.Any(x => x.Code == DiagnosticCode.InvalidModSide), Is.True);
    }
    
    private static BuildProperties Run(string text)
    {
        var c = new RecordingConsumer(text);
        new BuildTxtParser(text, c).Parse();
        return BuildTxtBinder.Bind(text, c.Properties, c.Diagnostics);
    }
}
