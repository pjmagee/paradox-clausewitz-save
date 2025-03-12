using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class MegastructureTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Megastructure_ReturnsAllMegastructures()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-megastructures"));

        // Act
        var megastructures = Megastructure.Load(gameStateDocument);

        // Assert
        Assert.IsNotNull(megastructures);
        Assert.IsTrue(megastructures.Length > 0);
        TestContext.WriteLine($"Found {megastructures.Length} megastructures");
    }

    [TestMethod]
    public void Megastructure_ParsesBasicPropertiesCorrectly()
    {
         // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-megastructures"));

        // Act
        var megastructures = Megastructure.Load(gameStateDocument);
        
        var megastructure = megastructures.First();

        // Assert
        Assert.AreEqual(520093697, megastructure.Id);
        Assert.AreEqual("dyson_sphere", megastructure.Type);
        Assert.AreEqual(1234, megastructure.Owner);
        Assert.IsNotNull(megastructure.Coordinate);
        Assert.AreEqual(0.75f, megastructure.BuildProgress);
        Assert.IsTrue(megastructure.IsActive);
    }
} 