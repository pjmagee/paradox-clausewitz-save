using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ExhibitTests
{
    public TestContext TestContext { get; set; } = null!;

    // MUST USE StellarisTestData.Save.Exhibits
    
    [TestMethod]
    public void Exhibit_ReturnsAllExhibits()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-exhibit"));
        var root = (SaveObject)gameState.Root;

        // Act
        var exhibits = Exhibit.Load(root);

        // Assert
        Assert.IsNotNull(exhibits);
        Assert.IsTrue(exhibits.Length > 0);
        TestContext.WriteLine($"Found {exhibits.Length} exhibits");
    }

    [TestMethod]
    public void Exhibit_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-exhibit"));
        var root = (SaveObject)gameState.Root;

        // Act
        var exhibits = Exhibit.Load(root);
        var firstExhibit = exhibits[0];

        // Assert
        Assert.AreEqual(1, firstExhibit.Id);
        Assert.AreEqual("test_exhibit", firstExhibit.Type);
        Assert.AreEqual(1, firstExhibit.Country);
        Assert.AreEqual(2, firstExhibit.Planet);
        Assert.IsTrue(firstExhibit.IsActive);
    }

    [TestMethod]
    public void Exhibit_ParsesVariablesCorrectly()
    {
        var exhibits = StellarisTestData.Save.Exhibits;
        
        // Test exhibit with variables (ID: 6)
        var e = exhibits.FirstOrDefault(e => e.Id == 6);
        Assert.IsNotNull(e, "Exhibit with variables should exist");
        
        // Test specimen properties
        Assert.AreEqual("key_to_fertility", e.Specimen.Specimen, "Exhibit should have correct specimen");
        Assert.AreEqual("A New Day Dawns", e.Specimen.Origin, "Exhibit should have correct origin");
        Assert.AreEqual("2236.04.02", e.Specimen.DateAdded, "Exhibit should have correct date added");
        
        // Test variables
        Assert.AreEqual(1, e.Specimen.DetailsVariables.Count, "Exhibit should have 1 details variable");
        Assert.AreEqual("Korinth Imperial Holdings", e.Specimen.DetailsVariables[0], "Exhibit should have correct details variable");
        
        Assert.AreEqual(1, e.Specimen.ShortVariables.Count, "Exhibit should have 1 short variable");
        Assert.AreEqual("Korinths", e.Specimen.ShortVariables[0], "Exhibit should have correct short variable");
        
        Assert.AreEqual(1, e.Specimen.NameVariables.Count, "Exhibit should have 1 name variable");
        Assert.AreEqual("Korinth", e.Specimen.NameVariables[0], "Exhibit should have correct name variable");
    }
} 