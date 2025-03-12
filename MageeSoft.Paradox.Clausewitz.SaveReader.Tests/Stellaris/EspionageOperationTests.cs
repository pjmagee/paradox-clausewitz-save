using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class EspionageOperationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void EspionageOperation_ReturnsAllOperations()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-espionage"));
        var root = (SaveObject)gameState.Root;

        // Act
        var operations = EspionageOperation.Load(root);

        // Assert
        Assert.IsNotNull(operations);
        Assert.IsTrue(operations.Length > 0);
        TestContext.WriteLine($"Found {operations.Length} operations");
    }

    [TestMethod]
    public void EspionageOperation_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-espionage"));
        var root = (SaveObject)gameState.Root;

        // Act
        var operations = EspionageOperation.Load(root);
        var firstOperation = operations[0];

        // Assert
        Assert.AreEqual(1, firstOperation.Id);
        Assert.AreEqual("test_operation", firstOperation.Type);
        Assert.AreEqual(1, firstOperation.Country);
        Assert.AreEqual(2, firstOperation.Target);
        Assert.IsTrue(firstOperation.IsActive);
    }
} 