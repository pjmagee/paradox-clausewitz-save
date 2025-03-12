using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ClusterTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Cluster_ReturnsAllClusters()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-clusters"));
        var root = (SaveObject)gameState.Root;

        // Act
        var clusters = Cluster.Load(gameState: root);

        // Assert
        Assert.IsNotNull(clusters);
        Assert.IsTrue(clusters.Length > 0);
        TestContext.WriteLine($"Found {clusters.Length} clusters");
    }

    [TestMethod]
    public void Cluster_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-clusters"));
        var root = (SaveObject)gameState.Root;

        // Act
        var clusters = Cluster.Load(gameState: root);
        var firstCluster = clusters[0];

        // Assert
        Assert.AreEqual(1, firstCluster.Id);
        Assert.AreEqual("test_cluster", firstCluster.Type);
        Assert.AreEqual(1, firstCluster.Country);
        Assert.IsTrue(firstCluster.IsActive);
        Assert.AreEqual(0.75f, firstCluster.Progress);
        Assert.IsNotNull(firstCluster.Resources);
        Assert.IsTrue(firstCluster.Resources.Count > 0);
    }
} 