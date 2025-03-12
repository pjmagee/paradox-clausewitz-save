using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ConstructionTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Construction_ReturnsAllConstructions()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-constructions"));
        var root = (SaveObject)gameState.Root;

        // Act
        var constructions = Construction.Load(gameState: root);

        // Assert
        Assert.IsNotNull(constructions);
        Assert.IsTrue(constructions.Length > 0);
        TestContext.WriteLine($"Found {constructions.Length} constructions");
    }

    [TestMethod]
    public void Construction_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-constructions"));
        var root = (SaveObject)gameState.Root;

        // Act
        var constructions = Construction.Load(gameState: root);
        var firstConstruction = constructions[0];

        // Assert
        Assert.AreEqual(1, firstConstruction.Id);
        Assert.AreEqual("building", firstConstruction.Type);
        Assert.AreEqual(1, firstConstruction.Planet);
        Assert.IsTrue(firstConstruction.IsActive);
        Assert.AreEqual(0.5f, firstConstruction.Progress);
        Assert.IsNotNull(firstConstruction.Resources);
        Assert.IsTrue(firstConstruction.Resources.Count > 0);
    }
} 