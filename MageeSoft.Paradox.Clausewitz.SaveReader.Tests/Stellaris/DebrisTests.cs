using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class DebrisTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Debris_ReturnsAllDebris()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-debris"));
        var root = (SaveObject)gameState.Root;

        // Act
        var debris = Debris.Load(root);

        // Assert
        Assert.IsNotNull(debris);
        Assert.IsTrue(debris.Length > 0);
        TestContext.WriteLine($"Found {debris.Length} debris");
    }

    [TestMethod]
    public void Debris_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-debris"));
        var root = (SaveObject)gameState.Root;

        // Act
        var debris = Debris.Load(root);
        var firstDebris = debris[0];

        // Assert
        Assert.AreEqual(1, firstDebris.Id);
        Assert.AreEqual(0, firstDebris.Country);
        Assert.AreEqual(0, firstDebris.FromCountry);
        Assert.IsNotNull(firstDebris.Resources);
        Assert.IsTrue(firstDebris.Resources.Count > 0);
        Assert.IsNotNull(firstDebris.ShipSizes);
        Assert.IsTrue(firstDebris.ShipSizes.Length > 0);
        Assert.IsNotNull(firstDebris.Components);
        Assert.IsTrue(firstDebris.Components.Length > 0);
        Assert.AreEqual("2200.01.01", firstDebris.Date);
        Assert.IsTrue(firstDebris.MustScavenge);
        Assert.IsFalse(firstDebris.MustReanimate);
        Assert.IsFalse(firstDebris.MustResearch);
    }
} 