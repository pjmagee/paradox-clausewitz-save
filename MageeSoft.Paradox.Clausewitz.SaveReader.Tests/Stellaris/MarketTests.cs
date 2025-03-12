using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class MarketTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Market_ReturnsAllMarkets()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-market"));

        // Act
        var markets = Market.Load(gameStateDocument);
        var count = markets.Length;

        // Assert
        Assert.IsNotNull(markets);
        Assert.IsTrue(count > 0);
        TestContext.WriteLine($"Found {count} markets");
    }

    [TestMethod]
    public void Market_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-market"));

        // Act
        var markets = Market.Load(gameStateDocument);
        var firstMarket = markets[0];

        // Assert
        Assert.AreEqual(1, firstMarket.Id);
        Assert.AreEqual("galactic", firstMarket.Type);
        Assert.AreEqual(1, firstMarket.Owner);
        Assert.IsNotNull(firstMarket.Resources);
        Assert.IsTrue(firstMarket.Resources.Count > 0);
        Assert.IsNotNull(firstMarket.Prices);
        Assert.IsTrue(firstMarket.Prices.Count > 0);
        Assert.IsNotNull(firstMarket.Demand);
        Assert.IsTrue(firstMarket.Demand.Count > 0);
    }
} 