using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class SpeciesTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Species_ReturnsAllSpecies()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-species"));

        // Act
        var species = Species.Load(gameStateDocument.Root);

        // Assert
        Assert.IsNotNull(species);
        Assert.IsTrue(species.Length > 0);
        TestContext.WriteLine($"Found {species.Length} species");
    }

    [TestMethod]
    public void Species_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-species"));

        // Act
        var species = Species.Load(gameStateDocument);
        var firstSpecies = species[0];

        // Assert
        Assert.AreEqual(1, firstSpecies.Id);
        Assert.AreEqual("human", firstSpecies.Class);
        Assert.AreEqual("human", firstSpecies.Portrait);
        Assert.IsNotNull(firstSpecies.Name);
        Assert.IsNotNull(firstSpecies.Traits);
        Assert.IsTrue(firstSpecies.Traits.Length > 0);
        Assert.AreEqual(1, firstSpecies.HomePlanet);
    }

    [TestMethod]
    public void Species_ParsesBasicProperties()
    {
        // Act
        var species = StellarisTestData.Save.Species;
        var human = species.FirstOrDefault(s => s.Id == 1);

        // Assert
        Assert.IsNotNull(human, "Human species should exist");
        Assert.AreEqual<string>("HUMAN1", human.NameList);
        Assert.AreEqual<string>("PRESCRIPTED_species_name_humans1", human.Name.ToString());
        Assert.AreEqual<string>("PRESCRIPTED_species_plural_humans1", human.NamePlural.ToString());
        Assert.AreEqual<string>("PRESCRIPTED_species_adjective_humans1", human.Adjective.ToString());
        Assert.AreEqual<string>("HUM", human.Class);
        Assert.AreEqual<string>("human", human.Portrait);
        Assert.AreEqual<long>(3L, human.HomePlanet);
        Assert.AreEqual<string>("not_set", human.Gender);
        Assert.AreEqual<int>(0, human.ExtraTraitPoints);
        Assert.IsFalse(human.Flags.Any(), "Human species should not have any flags");

        // Test traits
        Assert.IsTrue(human.Traits.Any(), "Human species should have traits");
        foreach (var trait in human.Traits)
        {
            TestContext.WriteLine($"Trait: {trait}");
        }

        // Test localized text with variables
        var localizedText = human.Name;
        Assert.IsNotNull(localizedText, "Localized text should not be null");
        Assert.AreEqual<string>("PRESCRIPTED_species_name_humans1", localizedText.ToString());

        // Test robotic species
        var roboticSpecies = species.FirstOrDefault(s => s.Class == "ROBOT");
        Assert.IsNotNull(roboticSpecies, "Robotic species should exist");
        Assert.AreEqual<string>("ROBOT", roboticSpecies.Class);
        Assert.AreEqual<string>("robot", roboticSpecies.Portrait);
        Assert.AreEqual<string>("not_set", roboticSpecies.Gender);

        // Test flags
        var flaggedSpecies = species.FirstOrDefault(s => s.Flags.Any());
        if (flaggedSpecies != null)
        {
            foreach (var flag in flaggedSpecies.Flags)
            {
                TestContext.WriteLine($"Flag: {flag.Key} = {flag.Value}");
            }
        }
    }

    [TestMethod]
    public void Species_ParsesTraits()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-species"));

        // Act
        var species = Species.Load(gameStateDocument);
        var firstSpecies = species.First(s => s.Id == 1);

        // Assert
        Assert.IsNotNull(firstSpecies.Traits);
        Assert.IsTrue(firstSpecies.Traits.Length > 0);
        Assert.IsTrue(firstSpecies.Traits.Contains("trait_adaptive"));
        Assert.IsTrue(firstSpecies.Traits.Contains("trait_pc_continental_preference"));
    }

    [TestMethod]
    public void Species_ParsesLocalizedTextWithVariables()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-species"));

        // Act
        var species = Species.Load(gameStateDocument);
        var refusoritor = species.FirstOrDefault(s => s.Name.Key.Contains("refusoritor"));

        // Assert
        Assert.IsNotNull(refusoritor, "Refusoritor species should exist");
        Assert.IsNotNull(refusoritor.Adjective, "Adjective should not be null");
        Assert.AreEqual("%ADJECTIVE%", refusoritor.Adjective.ToString());
        Assert.IsNotNull(refusoritor.Adjective.Variables, "Variables should not be null");
        Assert.IsTrue(refusoritor.Adjective.Variables.ContainsKey("adjective"), "Should have adjective variable");

        var adjVariable = refusoritor.Adjective.Variables["adjective"];
        Assert.IsNotNull(adjVariable, "Adjective variable should not be null");
        Assert.AreEqual("SPEC_Refusoritor", adjVariable.Value.ToString());

        // Additional assertions for other localized text
        Assert.AreEqual("SPEC_Refusoritor", refusoritor.Name.ToString());
        Assert.AreEqual("SPEC_Refusoritor_pl", refusoritor.NamePlural.ToString());
    }

    [TestMethod]
    public void Species_ParsesRoboticSpecies()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-species"));

        // Act
        var species = Species.Load(gameStateDocument);
        var robot = species.FirstOrDefault(s => s.Id == 6L && s.Class == "ROBOT");

        // Assert
        Assert.IsNotNull(robot, "Robot species should exist");
        Assert.AreEqual("ROBOT", robot.Class);
        Assert.IsTrue(robot.Portrait.Contains("machine") || robot.Portrait.Contains("robot"), "Robot portrait should contain 'machine' or 'robot'");
        Assert.IsTrue(robot.Traits.Contains("trait_mechanical"), "Robot should have mechanical trait");
        Assert.IsTrue(robot.Traits.Any(t => t.Contains("planet_preference")), "Robot should have a planet preference trait");
        Assert.AreEqual(6L, robot.Id, "Robot should have ID 6");
    }

    [TestMethod]
    public void Species_ParsesFlags()
    {
        // Arrange
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-species"));

        // Act
        var species = Species.Load(gameStateDocument);

        // Log all species for debugging
        foreach (var s in species)
        {
            TestContext.WriteLine($"Species: Id={s.Id}, Class={s.Class}, Portrait={s.Portrait}, Flags={string.Join(", ", s.Flags.Select(f => $"{f.Key}={f.Value}"))}");
        }

        var robotWithFlags = species.FirstOrDefault(s => s.Id == 142);

        // Assert
        Assert.IsNotNull(robotWithFlags, "Robot species with flags should exist");
        Assert.IsTrue(robotWithFlags.Flags.Count > 0, "Should have at least one flag");
        Assert.IsTrue(robotWithFlags.Flags.ContainsKey("mechanical_species6"), "Should have mechanical_species6 flag");
        Assert.AreEqual(63078744L, robotWithFlags.Flags["mechanical_species6"]);
        Assert.AreEqual("ROBOT", robotWithFlags.Class);
        Assert.AreEqual("sd_rep_robot", robotWithFlags.Portrait);
        Assert.IsTrue(robotWithFlags.Traits.Contains("trait_mechanical"), "Should have mechanical trait");
        Assert.IsTrue(robotWithFlags.Traits.Contains("trait_frozen_planet_preference"), "Should have frozen planet preference");
    }
} 