using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ArchaeologicalSiteTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void ArchaeologicalSite_ReturnsAllSites()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-archaeological"));

        // Act
        var sites = ArchaeologicalSite.Load(gameState.Root);

        // Assert
        Assert.IsNotNull(sites);
        Assert.IsTrue(sites.Length > 0);
        TestContext.WriteLine($"Found {sites.Length} archaeological sites");
    }

    [TestMethod]
    public void ArchaeologicalSite_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-archaeological"));
        var root = (SaveObject)gameState.Root;

        // Act
        var sites = ArchaeologicalSite.Load(root);
        var firstSite = sites[0];

        // Assert
        Assert.AreEqual(1, firstSite.Id);
        Assert.AreEqual("test_site", firstSite.Type);
        //Assert.AreEqual(1, firstSite.Country);
        Assert.AreEqual(2, firstSite.Planet);
        Assert.IsTrue(firstSite.IsActive);
    }
} 