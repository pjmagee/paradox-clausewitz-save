using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class PlanetTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Planets_ReturnsAllPlanets()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-planets"));
        var root = (SaveObject)gameState.Root;

        // Act
        var planets = Planet.Load(root);

        // Assert
        Assert.IsNotNull(planets);
        Assert.IsTrue(planets.Length > 0);
        TestContext.WriteLine($"Found {planets.Length} planets");
        
        // Debug output for all planet IDs
        foreach (var planet in planets)
        {
            TestContext.WriteLine($"Found planet with ID {planet.Id}, Name: {planet.Name?.Key ?? "null"}, Class: {planet.PlanetClass}");
        }
    }

    [TestMethod]
    public void Planet_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-planets"));
        var root = (SaveObject)gameState.Root;

        // Act
        var planets = Planet.Load(root);
        
        // Test Sol (ID: 0)
        var sol = planets.FirstOrDefault(p => p.Id == 0);
        Assert.IsNotNull(sol, "Sol should exist");
        Assert.AreEqual("NAME_Sol", sol.Name.Key, "Sol should have correct name key");
        Assert.AreEqual("pc_g_star", sol.PlanetClass, "Sol should be a G-class star");
        Assert.AreEqual(30, sol.PlanetSize, "Sol should have size 30");
        Assert.AreEqual(0.0f, sol.Orbit, "Sol should have orbit value 0");
        Assert.IsTrue(sol.PreventAnomaly, "Sol should prevent anomalies");
        Assert.IsTrue(sol.Deposits.Contains(17), "Sol should have deposit 17");
        Assert.AreEqual(0, sol.BombardmentDamage, "Sol should have 0 bombardment damage");
        Assert.AreEqual("0.01.01", sol.LastBombardment, "Sol should have correct last bombardment date");
        Assert.IsFalse(sol.AutomatedDevelopment, "Sol should not have automated development");
        
        // Test Mercury (ID: 1)
        var mercury = planets.FirstOrDefault(p => p.Id == 1);
        Assert.IsNotNull(mercury, "Mercury should exist");
        Assert.AreEqual("NAME_Mercury", mercury.Name.Key, "Mercury should have correct name key");
        Assert.AreEqual("pc_molten", mercury.PlanetClass, "Mercury should be a molten planet");
        Assert.AreEqual(10, mercury.PlanetSize, "Mercury should have size 10");
        Assert.AreEqual(40.0f, mercury.Orbit, "Mercury should have orbit value 40");
        Assert.IsTrue(mercury.PreventAnomaly, "Mercury should prevent anomalies");
        Assert.IsTrue(mercury.Deposits.Contains(21), "Mercury should have deposit 21");
    }
} 
