using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class DepositTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Deposit_ReturnsAllDeposits()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-deposit"));
        // Act
        var deposits = Deposit.Load(gameState.Root);

        // Assert
        Assert.IsNotNull(deposits);
        Assert.IsTrue(deposits.Length > 0);
        TestContext.WriteLine($"Found {deposits.Length} deposits");
    }

    [TestMethod]
    public void Deposit_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-deposit"));
        var root = (SaveObject)gameState.Root;

        // Act
        var deposits = Deposit.Load(root);
        var firstDeposit = deposits[0];

        // Assert
        Assert.AreEqual(1, firstDeposit.Id);
        Assert.AreEqual("test_deposit", firstDeposit.Type);
        Assert.AreEqual(1, firstDeposit.Planet);
        Assert.AreEqual(2, firstDeposit.Amount);
        Assert.IsTrue(firstDeposit.IsActive);
    }
} 