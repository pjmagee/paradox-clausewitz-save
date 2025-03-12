using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ShipTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Ship_ReturnsAllShips()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-ships"));
        var root = (SaveObject)gameState.Root;

        // Act
        var ships = Ship.Load(gameState: root);

        // Assert
        Assert.IsNotNull(ships);
        Assert.IsTrue(ships.Length > 0);
        TestContext.WriteLine($"Found {ships.Length} ships");

        // Debug output for all ship IDs
        foreach (var ship in ships)
        {
            TestContext.WriteLine($"Found ship with ID {ship.Id}");
        }
    }

    [TestMethod]
    public void Ship_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-ships"));
        var root = (SaveObject)gameState.Root;

        // Act
        var ships = Ship.Load(gameState: root);
        var firstShip = ships[0];

        // Assert
        Assert.AreEqual(1, firstShip.Id);
        Assert.AreEqual("corvette", firstShip.ShipSize);
        Assert.AreEqual(1, firstShip.Fleet);
        Assert.IsTrue(firstShip.IsActive);
        Assert.AreEqual(100.0f, firstShip.Health);
        Assert.AreEqual(100.0f, firstShip.MaxHealth);
        Assert.IsNotNull(firstShip.Components);
        Assert.IsTrue(firstShip.Components.Count > 0);
    }
} 