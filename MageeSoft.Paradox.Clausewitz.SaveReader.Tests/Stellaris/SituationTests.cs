using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class SituationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Situation_ReturnsAllSituations()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-situation"));
        var root = (SaveObject)gameState.Root;

        // Act
        var situations = Situation.Load(root);

        // Assert
        Assert.IsNotNull(situations);
        Assert.IsTrue(situations.Length > 0);
        TestContext.WriteLine($"Found {situations.Length} situations");
    }

    [TestMethod]
    public void Situation_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-situation"));
        var root = (SaveObject)gameState.Root;

        // Act
        var situations = Situation.Load(root);
        var firstSituation = situations[0];

        // Assert
        Assert.AreEqual(1, firstSituation.Id);
        Assert.AreEqual("test_situation", firstSituation.Type);
        Assert.AreEqual(1, firstSituation.Country);
        Assert.AreEqual(2, firstSituation.Value);
        Assert.IsTrue(firstSituation.IsActive);
    }
} 