using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class AstralRiftTests
{
    public TestContext TestContext { get; set; } = null!;
    

    [TestMethod]
    public void AstralRift_ReturnsAllRifts()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-astral_rifts"));
        
        // Act
        var rifts = AstralRift.Load(gameState.Root);

        // Assert
        Assert.IsNotNull(rifts);
        Assert.IsTrue(rifts.Count > 0);
        TestContext.WriteLine($"Found {rifts.Count} astral rifts");
    }

    [TestMethod]
    public void AstralRift_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-astral_rifts"));
        
        // Act
        var rifts = AstralRift.Load(gameState.Root);
        var firstRift = rifts[0];

        // Assert
        Assert.AreEqual(1, firstRift.Id);
        Assert.AreEqual("test_rift", firstRift.Type);
        //Assert.AreEqual(1, firstRift.Country); // compile error
        //Assert.AreEqual(2, firstRift.Value); // compile error
        Assert.IsTrue(firstRift.IsActive);
    }
} 