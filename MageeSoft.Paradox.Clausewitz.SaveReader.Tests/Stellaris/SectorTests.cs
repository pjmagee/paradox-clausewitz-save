using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class SectorTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Sector_ReturnsAllSectors()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-sectors"));

        // Act
        var sectors = Sector.Load(gameStateDocument);

        // Assert
        Assert.IsNotNull(sectors);
        Assert.IsTrue(sectors.Length > 0);
        TestContext.WriteLine($"Found {sectors.Length} sectors");
    }

    [TestMethod]
    public void Sector_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-sectors"));

        // Act
        var sectors = Sector.Load(gameStateDocument);
        var firstSector = sectors[0];

        // Assert
        Assert.AreEqual(1, firstSector.Id);
        Assert.AreEqual(1, firstSector.Owner);
        Assert.IsTrue(firstSector.Systems.Length > 0);
        Assert.IsFalse(string.IsNullOrEmpty(firstSector.Name));
    }

    [TestMethod]
    public void Sector_ParsesSystemsCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-sectors"));

        // Act
        var sectors = Sector.Load(gameStateDocument);
        var firstSector = sectors[0];

        // Assert
        Assert.IsTrue(firstSector.Systems.Length > 0);

        // Log systems for debugging
        TestContext.WriteLine("Systems:");
        foreach (var system in firstSector.Systems)
        {
            TestContext.WriteLine($"System ID: {system}");
        }
    }

    [TestMethod]
    public void Sector_ParsesResourcesCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-sectors"));

        // Act
        var sectors = Sector.Load(gameStateDocument);
        var firstSector = sectors[0];

        // Assert
        Assert.IsNotNull(firstSector.Resources);

        // Log resources for debugging
        TestContext.WriteLine("Resources:");
        foreach (var resource in firstSector.Resources)
        {
            TestContext.WriteLine($"{resource.Key}: {resource.Value}");
        }
    }

    [TestMethod]
    public void Sector_ParsesStockpileCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-sectors"));

        // Act
        var sectors = Sector.Load(gameStateDocument);
        var firstSector = sectors[0];

        // Assert
        Assert.IsNotNull(firstSector.Stockpile);

        // Log stockpile for debugging
        TestContext.WriteLine("Stockpile:");
        foreach (var stockpile in firstSector.Stockpile)
        {
            TestContext.WriteLine($"{stockpile.Key}: {stockpile.Value}");
        }
    }
} 