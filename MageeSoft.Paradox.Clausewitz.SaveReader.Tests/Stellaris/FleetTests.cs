using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class FleetTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Fleet_ReturnsAllFleets()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-fleet"));
        var root = (SaveObject)gameState.Root;

        // Act
        var fleets = Fleet.Load(gameState: root);

        // Assert
        Assert.IsNotNull(fleets);
        Assert.IsTrue(fleets.Length > 0);
        TestContext.WriteLine($"Found {fleets.Length} fleets");
    }

    [TestMethod]
    public void Fleet_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-fleet"));
        var root = (SaveObject)gameState.Root;

        // Act
        var fleets = Fleet.Load(gameState: root);
        var firstFleet = fleets[0];

        // Assert
        Assert.AreEqual(1, firstFleet.Id);
        Assert.AreEqual("combat", firstFleet.Type);
        Assert.AreEqual(1, firstFleet.OwnerId);
        Assert.IsNotNull(firstFleet.Position);
        Assert.IsNotNull(firstFleet.Ships);
        Assert.IsTrue(firstFleet.Ships.Length > 0);
    }
} 