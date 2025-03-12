using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class AchievementsTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Achievement_ReturnsAllAchievements()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-achievements"));

        // Act
        var achievements = Achievements.Load(gameState.Root);

        // Assert
        Assert.IsNotNull(achievements);
        Assert.IsNotNull(achievements.AchievementIds);
        Assert.IsTrue(achievements.AchievementIds.Length > 0);
        
        TestContext.WriteLine("There are {0} achievements", achievements.AchievementIds.Length);
    }

    [TestMethod]
    public void Achievement_ContainsExpectedIds()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-achievements"));

        // Act
        var achievements = Achievements.Load(gameStateDocument.Root);

        // Assert
        // Test for some specific achievement IDs we know should be present
        Assert.IsTrue(achievements.AchievementIds.Contains(22), "Achievement ID 22 not found");
        Assert.IsTrue(achievements.AchievementIds.Contains(27), "Achievement ID 27 not found");
        Assert.IsTrue(achievements.AchievementIds.Contains(30), "Achievement ID 30 not found");
        
        // Test that IDs are in ascending order
        var sortedIds = achievements.AchievementIds.OrderBy(id => id).ToArray();
        CollectionAssert.AreEqual(sortedIds, achievements.AchievementIds.ToArray());
    }

    [TestMethod]
    public void Achievement_HasValidIdRange()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-achievements"));

        // Act
        var achievements = Achievements.Load(gameStateDocument.Root);

        // Assert
        // All achievement IDs should be positive
        Assert.IsTrue(achievements.AchievementIds.All(id => id > 0));
        
        // Check that we don't have any duplicates
        Assert.AreEqual(achievements.AchievementIds.Length, achievements.AchievementIds.Distinct().Count());
    }
} 