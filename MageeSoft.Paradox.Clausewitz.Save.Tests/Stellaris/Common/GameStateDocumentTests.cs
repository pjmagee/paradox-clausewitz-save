using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Reader;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Stellaris.Common;

[TestClass]
public class GameStateDocumentTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void TestGameStateParse()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        Assert.IsTrue(root.Properties.Any(), "Root should have properties");

        // Output root properties for debugging
        foreach (var prop in root.Properties)
        {
            TestContext.WriteLine($"{prop.Key} = {prop.Value}");
        }
    }
    
    [TestMethod]
    public void TestMetaParse()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "meta")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root;
        Assert.IsNotNull(root);
        Assert.IsTrue(root.Properties.Any(), "Root should have properties");
        
        foreach (var prop in root.Properties)
        {
            TestContext.WriteLine($"{prop.Key} = {prop.Value}");
        }
    }
}