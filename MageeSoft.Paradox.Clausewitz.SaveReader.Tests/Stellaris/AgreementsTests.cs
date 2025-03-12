using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class AgreementsTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Agreement_ReturnsAllAgreements()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-agreements"));
        var root = (SaveObject)gameState.Root;

        // Act
        var agreements = Agreement.Load(root);

        // Assert
        Assert.IsNotNull(agreements);
        Assert.IsTrue(agreements.Length > 0);
        TestContext.WriteLine($"Found {agreements.Length} agreements");
    }

    [TestMethod]
    public void Agreement_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-agreements"));
        var root = (SaveObject)gameState.Root;

        // Act
        var agreements = Agreement.Load(root);
        var firstAgreement = agreements[0];

        // Assert
        Assert.AreEqual(1, firstAgreement.Id);
        Assert.AreEqual("federation", firstAgreement.Type);
        Assert.AreEqual(1, firstAgreement.First);
        Assert.AreEqual(2, firstAgreement.Second);
        Assert.AreEqual(new DateOnly(2200, 01, 01), firstAgreement.StartDate);
    }

    [TestMethod]
    public void Agreement_ParsesTermsCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-agreements"));
        var root = (SaveObject)gameState.Root;

        // Act
        var agreements = Agreement.Load(root);
        var firstAgreement = agreements[0];

        // Assert
        Assert.IsNotNull(firstAgreement.Terms);
        Assert.AreEqual("overlord", firstAgreement.Terms.Type);
        Assert.AreEqual(1, firstAgreement.Terms.Level);
        Assert.AreEqual(0, firstAgreement.Terms.Length);
        Assert.AreEqual(10L, firstAgreement.Terms.First);
        Assert.AreEqual(71L, firstAgreement.Terms.Second);

        // Check resources
        Assert.AreEqual(0f, firstAgreement.Terms.FirstResources["energy"]);
        Assert.AreEqual(0f, firstAgreement.Terms.FirstResources["minerals"]);
        Assert.AreEqual(0f, firstAgreement.Terms.SecondResources["energy"]);
        Assert.AreEqual(0f, firstAgreement.Terms.SecondResources["minerals"]);

        // Check modifiers
        Assert.AreEqual(0f, firstAgreement.Terms.FirstModifiers["subject_power"]);
        Assert.AreEqual(0f, firstAgreement.Terms.FirstModifiers["overlord_power"]);
        Assert.AreEqual(0f, firstAgreement.Terms.SecondModifiers["subject_power"]);
        Assert.AreEqual(0f, firstAgreement.Terms.SecondModifiers["overlord_power"]);
    }

    [TestMethod]
    public void Agreement_ParsesResourcesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-agreements"));
        var root = (SaveObject)gameState.Root;

        // Act
        var agreements = Agreement.Load(root);
        var firstAgreement = agreements[0];

        // Assert
        Assert.IsNotNull(firstAgreement, "First agreement should not be null");
        Assert.IsNotNull(firstAgreement.Terms, "Terms should not be null");

        TestContext.WriteLine("First Party Resources:");
        foreach (var resource in firstAgreement.Terms.FirstResources)
        {
            TestContext.WriteLine("{0}: {1}", resource.Key, resource.Value);
        }

        TestContext.WriteLine("Second Party Resources:");
        foreach (var resource in firstAgreement.Terms.SecondResources)
        {
            TestContext.WriteLine("{0}: {1}", resource.Key, resource.Value);
        }
        
        Assert.AreEqual(0f, firstAgreement.Terms.FirstResources["energy"], "First party energy should be 0");
        Assert.AreEqual(0f, firstAgreement.Terms.FirstResources["minerals"], "First party minerals should be 0");
        Assert.AreEqual(0f, firstAgreement.Terms.SecondResources["energy"], "Second party energy should be 0");
        Assert.AreEqual(0f, firstAgreement.Terms.SecondResources["minerals"], "Second party minerals should be 0");
    }

    [TestMethod]
    public void Agreement_ParsesModifiersCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-agreements"));
        var root = (SaveObject)gameState.Root;

        // Act
        var agreements = Agreement.Load(root);
        var firstAgreement = agreements[0];

        // Assert
        Assert.IsNotNull(firstAgreement, "First agreement should not be null");
        Assert.IsNotNull(firstAgreement.Terms, "Terms should not be null");

        TestContext.WriteLine("First Party Modifiers:");
        foreach (var modifier in firstAgreement.Terms.FirstModifiers)
        {
            TestContext.WriteLine("{0}: {1}", modifier.Key, modifier.Value);
        }

        TestContext.WriteLine("Second Party Modifiers:");
        foreach (var modifier in firstAgreement.Terms.SecondModifiers)
        {
            TestContext.WriteLine("{0}: {1}", modifier.Key, modifier.Value);
        }
        
        Assert.AreEqual(0f, firstAgreement.Terms.FirstModifiers["subject_power"], "First party subject_power should be 0");
        Assert.AreEqual(0f, firstAgreement.Terms.FirstModifiers["overlord_power"], "First party overlord_power should be 0");
        Assert.AreEqual(0f, firstAgreement.Terms.SecondModifiers["subject_power"], "Second party subject_power should be 0");
        Assert.AreEqual(0f, firstAgreement.Terms.SecondModifiers["overlord_power"], "Second party overlord_power should be 0");
    }
} 