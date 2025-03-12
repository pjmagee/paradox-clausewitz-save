using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class StormTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Storm_ReturnsAllStorms()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-storm"));
        var root = (SaveObject)gameState.Root;

        // Act
        var storms = Storm.Load(gameState: root);

        // Assert
        Assert.IsNotNull(storms);
        Assert.IsTrue(storms.Length > 0);
        TestContext.WriteLine($"Found {storms.Length} storms");
    }

    [TestMethod]
    public void Storm_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-storm"));
        var root = (SaveObject)gameState.Root;

        // Act
        var storms = Storm.Load(gameState: root);
        var firstStorm = storms[0];

        // Assert
        Assert.AreEqual(1, firstStorm.Id);
        Assert.AreEqual("test_storm", firstStorm.Type);
        Assert.AreEqual(1, firstStorm.Country);
        Assert.AreEqual(2, firstStorm.Value);
        Assert.IsTrue(firstStorm.IsActive);
    }
} 