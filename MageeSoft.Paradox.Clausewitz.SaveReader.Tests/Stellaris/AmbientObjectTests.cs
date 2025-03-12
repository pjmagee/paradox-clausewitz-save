using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class AmbientObjectTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void AmbientObject_ReturnsAllAmbientObjects()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-ambientobjects"));
        var root = (SaveObject)gameState.Root;

        // Act
        var objects = AmbientObject.Load(root);

        // Assert
        Assert.IsNotNull(objects);
        Assert.IsTrue(objects.Length > 0);
        TestContext.WriteLine($"Found {objects.Length} ambient objects");
    }

    [TestMethod]
    public void AmbientObject_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-ambientobjects"));

        // Act
        var objects = AmbientObject.Load(gameState.Root);
        var firstObject = objects[0];

        // Assert
        Assert.AreEqual(1, firstObject.Id);
        Assert.AreEqual("nebula", firstObject.Type);
        Assert.IsNotNull(firstObject.Coordinate);
        Assert.IsTrue(firstObject.Coordinate.X != 0 || firstObject.Coordinate.Y != 0);
        Assert.IsNotNull(firstObject.Properties);
        Assert.IsNotNull(firstObject.Properties.Coordinate);
    }
} 