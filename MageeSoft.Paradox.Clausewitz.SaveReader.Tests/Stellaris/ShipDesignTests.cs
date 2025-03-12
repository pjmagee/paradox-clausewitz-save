using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ShipDesignTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void ShipDesign_ReturnsAllShipDesigns()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-shipdesigns"));

        // Act
        var designs = ShipDesign.Load(gameStateDocument);

        // Assert
        Assert.IsNotNull(designs);
        Assert.IsTrue(designs.Length > 0);
        TestContext.WriteLine($"Found {designs.Length} ship designs");
    }

    [TestMethod]
    public void ShipDesign_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-shipdesigns"));

        // Act
        var designs = ShipDesign.Load(gameStateDocument);
        var firstDesign = designs[0];

        // Assert
        Assert.AreEqual(1, firstDesign.Id);
        Assert.AreEqual("corvette", firstDesign.ShipSize);
        Assert.IsNotNull(firstDesign.Components);
        Assert.IsTrue(firstDesign.Components.Count > 0);
    }

    [TestMethod]
    public void ShipDesign_ParsesComponentsCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-shipdesigns"));

        // Act
        var designs = ShipDesign.Load(gameStateDocument);
        var firstDesign = designs[0];

        // Assert
        Assert.IsNotNull(firstDesign.Components);
        Assert.IsTrue(firstDesign.Components.Count > 0);

        // Log components for debugging
        TestContext.WriteLine("Components:");
        foreach (var component in firstDesign.Components)
        {
            TestContext.WriteLine($"{component.Key}: {component.Value}");
        }
    }

    [TestMethod]
    public void ShipDesign_ParsesWeaponsAndUtilitiesCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-shipdesigns"));

        // Act
        var designs = ShipDesign.Load(gameStateDocument);
        var firstDesign = designs[0];

        // Assert
        Assert.IsNotNull(firstDesign.Weapons);
        Assert.IsNotNull(firstDesign.Utilities);

        // Log weapons for debugging
        TestContext.WriteLine("Weapons:");
        foreach (var weapon in firstDesign.Weapons)
        {
            TestContext.WriteLine($"{weapon.Key}: {weapon.Value}");
        }

        // Log utilities for debugging
        TestContext.WriteLine("Utilities:");
        foreach (var utility in firstDesign.Utilities)
        {
            TestContext.WriteLine($"{utility.Key}: {utility.Value}");
        }
    }

    [TestMethod]
    public void ShipDesign_ParsesCostAndStatsCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-shipdesigns"));

        // Act
        var designs = ShipDesign.Load(gameStateDocument);
        var firstDesign = designs[0];

        // Assert
        Assert.IsNotNull(firstDesign.Cost);
        Assert.IsNotNull(firstDesign.Stats);

        // Log cost for debugging
        TestContext.WriteLine("Cost:");
        foreach (var cost in firstDesign.Cost)
        {
            TestContext.WriteLine($"{cost.Key}: {cost.Value}");
        }

        // Log stats for debugging
        TestContext.WriteLine("Stats:");
        foreach (var stat in firstDesign.Stats)
        {
            TestContext.WriteLine($"{stat.Key}: {stat.Value}");
        }
    }
} 