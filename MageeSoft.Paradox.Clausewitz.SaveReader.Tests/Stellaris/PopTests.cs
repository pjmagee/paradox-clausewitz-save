using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class PopTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Pops_ReturnsAllPops()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-pops"));
        var root = (SaveObject)gameState.Root;

        // Act
        var pops = Pop.Load(gameState: root);

        // Assert
        Assert.IsNotNull(pops);
        Assert.IsTrue(pops.Length > 0);
        TestContext.WriteLine($"Found {pops.Length} pops");
    }

    [TestMethod]
    public void Pop_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-pops"));
        var root = (SaveObject)gameState.Root;

        // Act
        var pops = Pop.Load(gameState: root);
        var firstPop = pops[0];

        // Assert
        Assert.AreEqual(1, firstPop.Id);
        Assert.AreEqual(1, firstPop.Planet);
        Assert.AreEqual(1, firstPop.Species);
        Assert.IsNotNull(firstPop.Job);
        Assert.IsNotNull(firstPop.Ethos);
        Assert.IsTrue(firstPop.Ethos.Count > 0);
    }
} 