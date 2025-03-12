using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ArmyTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Army_ReturnsAllArmies()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-army"));

        // Act
        var armies = Army.Load(gameState.Root);

        // Assert
        Assert.IsNotNull(armies);
        Assert.IsTrue(armies.Length > 0);
        TestContext.WriteLine($"Found {armies.Length} armies");
    }

    [TestMethod]
    public void Army_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-army"));

        // Act
        var armies = Army.Load(gameState.Root);
        var firstArmy = armies[0];

        // Assert
        Assert.AreEqual(1, firstArmy.Id);
        Assert.AreEqual("assault_army", firstArmy.Type);
        Assert.AreEqual(1, firstArmy.Country);
        Assert.AreEqual(2, firstArmy.Planet);
        Assert.IsTrue(firstArmy.IsActive);
    }
} 