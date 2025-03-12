using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class BypassTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Bypass_ReturnsAllBypasses()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-bypasses"));
        var root = (SaveObject)gameState.Root;

        // Act
        var bypasses = Bypass.Load(root);

        // Assert
        Assert.IsNotNull(bypasses);
        Assert.IsTrue(bypasses.Length > 0);
        TestContext.WriteLine($"Found {bypasses.Length} bypasses");
    }

    [TestMethod]
    public void Bypass_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-bypasses"));
        var root = (SaveObject)gameState.Root;

        // Act
        var bypasses = Bypass.Load(root);
        var firstBypass = bypasses[0];

        // Assert
        Assert.AreEqual(1, firstBypass.Id);
        Assert.AreEqual("wormhole", firstBypass.Type);
        Assert.IsNotNull(firstBypass.Coordinate);
        Assert.IsTrue(firstBypass.Coordinate.X != 0 || firstBypass.Coordinate.Y != 0);
        Assert.IsNotNull(firstBypass.LinkedTo);
        Assert.IsTrue(firstBypass.LinkedTo.Length > 0);
    }
} 