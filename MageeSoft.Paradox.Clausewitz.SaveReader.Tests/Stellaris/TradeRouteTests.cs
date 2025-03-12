using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class TradeRouteTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void TradeRoute_ReturnsAllRoutes()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-trade"));
        var root = (SaveObject)gameState.Root;

        // Act
        var routes = TradeRoute.Load(root);

        // Assert
        Assert.IsNotNull(routes);
        Assert.IsTrue(routes.Length > 0);
        TestContext.WriteLine($"Found {routes.Length} trade routes");
    }

    [TestMethod]
    public void TradeRoute_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-trade"));
        var root = (SaveObject)gameState.Root;

        // Act
        var routes = TradeRoute.Load(root);
        var firstRoute = routes[0];

        // Assert
        Assert.AreEqual(1, firstRoute.Id);
        Assert.AreEqual(1, firstRoute.Owner);
        Assert.AreEqual(2, firstRoute.Value);
        Assert.IsTrue(firstRoute.IsActive);
    }
} 