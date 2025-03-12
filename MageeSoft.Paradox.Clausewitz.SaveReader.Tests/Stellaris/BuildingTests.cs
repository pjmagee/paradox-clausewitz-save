using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class BuildingTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Building_ReturnsAllBuildings()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-buildings"));
        var root = (SaveObject)gameState.Root;

        // Act
        var buildings = Building.Load(root);

        // Assert
        Assert.IsNotNull(buildings);
        Assert.IsTrue(buildings.Length > 0);
        TestContext.WriteLine($"Found {buildings.Length} buildings");
    }

    [TestMethod]
    public void Building_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-buildings"));
        var root = (SaveObject)gameState.Root;

        // Act
        var buildings = Building.Load(root);
        var firstBuilding = buildings[0];

        // Assert
        Assert.AreEqual(1, firstBuilding.Id);
        Assert.AreEqual("capital", firstBuilding.Type);
        Assert.AreEqual(1, firstBuilding.Planet);
        Assert.AreEqual(100.0f, firstBuilding.Health);
        Assert.AreEqual(100.0f, firstBuilding.MaxHealth);
        Assert.IsTrue(firstBuilding.IsActive);
    }
} 