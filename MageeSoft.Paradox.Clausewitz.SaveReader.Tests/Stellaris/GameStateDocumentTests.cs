using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class GameStateDocumentTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void TestParse()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        Assert.IsTrue(root.Properties.Any(), "Root should have properties");

        // Output root properties for debugging
        foreach (var prop in root.Properties)
        {
            TestContext.WriteLine($"{prop.Key} = {prop.Value}");
        }

        // Verify some expected properties
        Assert.IsTrue(root.Properties.Any(p => p.Key == "galaxy"), "Should have galaxy property");
        Assert.IsTrue(root.Properties.Any(p => p.Key == "player"), "Should have player property");
        Assert.IsTrue(root.Properties.Any(p => p.Key == "country"), "Should have country property");
    }

    [TestMethod]
    public void TestParseMarketFile()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-market")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var marketData = root.Properties.Single();
        Assert.AreEqual("market", marketData.Key);

        // Verify market object structure
        var market = marketData.Value as SaveObject;
        Assert.IsNotNull(market);
        var marketProps = market.Properties.ToList();
        Assert.IsTrue(marketProps.Any());

        // Output market data for debugging
        foreach (var prop in marketProps)
        {
            TestContext.WriteLine($"{prop.Key} = {prop.Value}");
        }
    }

    [TestMethod]
    public void TestParseSpeciesDb()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-species_db")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var speciesData = root.Properties.Single();
        Assert.AreEqual("species_db", speciesData.Key);

        // Verify species object structure
        var speciesDb = speciesData.Value as SaveObject;
        Assert.IsNotNull(speciesDb);
        var speciesProps = speciesDb.Properties.ToList();
        Assert.IsTrue(speciesProps.Any());

        // Output species data for debugging
        foreach (var species in speciesProps)
        {
            TestContext.WriteLine($"Species {species.Key}:");
            var speciesObj = species.Value as SaveObject;
            if (speciesObj != null)
            {
                foreach (var prop in speciesObj.Properties)
                {
                    TestContext.WriteLine($"  {prop.Key} = {prop.Value}");
                }
            }
        }
    }

    [TestMethod]
    public void TestParseEventTargets()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-saved_event_target")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var eventData = root.Properties.Single();
        Assert.AreEqual("saved_event_target", eventData.Key);

        // Verify event target object structure
        var eventTargets = eventData.Value as SaveObject;
        Assert.IsNotNull(eventTargets);
        var eventProps = eventTargets.Properties.ToList();
        Assert.IsTrue(eventProps.Any());

        // Output event target data for debugging
        foreach (var target in eventProps)
        {
            TestContext.WriteLine($"Event Target {target.Key}:");
            var targetObj = target.Value as SaveObject;
            if (targetObj != null)
            {
                foreach (var prop in targetObj.Properties)
                {
                    TestContext.WriteLine($"  {prop.Key} = {prop.Value}");
                }
            }
        }
    }

    [TestMethod]
    public void TestParseStorms()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-storms")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var stormsData = root.Properties.Single();
        Assert.AreEqual("storms", stormsData.Key);

        // Verify storms object structure
        var storms = stormsData.Value as SaveObject;
        Assert.IsNotNull(storms);
        var stormProps = storms.Properties.ToList();
        Assert.IsTrue(stormProps.Any());

        // Output storm data for debugging
        foreach (var storm in stormProps)
        {
            TestContext.WriteLine($"Storm {storm.Key}:");
            var stormObj = storm.Value as SaveObject;
            if (stormObj != null)
            {
                foreach (var prop in stormObj.Properties)
                {
                    TestContext.WriteLine($"  {prop.Key} = {prop.Value}");
                }
            }
        }
    }

    [TestMethod]
    public void TestParseMarketFluctuations()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-market")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var marketData = root.Properties.Single();
        Assert.AreEqual("market", marketData.Key);

        // Verify market object structure
        var market = marketData.Value as SaveObject;
        Assert.IsNotNull(market);
        var marketProps = market.Properties.ToList();
        Assert.IsTrue(marketProps.Any());

        // Output market data for debugging
        foreach (var prop in marketProps)
        {
            TestContext.WriteLine($"{prop.Key} = {prop.Value}");
        }
    }

    [TestMethod]
    public void TestParseResourceAmounts()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-market")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var marketData = root.Properties.Single();
        Assert.AreEqual("market", marketData.Key);

        // Verify market object structure
        var market = marketData.Value as SaveObject;
        Assert.IsNotNull(market);
        var marketProps = market.Properties.ToList();
        Assert.IsTrue(marketProps.Any());

        // Output market data for debugging
        foreach (var prop in marketProps)
        {
            TestContext.WriteLine($"{prop.Key} = {prop.Value}");
        }
    }

    [TestMethod]
    public void TestParseSpeciesPortrait()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-used_species_portrait")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var portraitData = root.Properties.Single();
        Assert.AreEqual("used_species_portrait", portraitData.Key);

        // Verify portrait object structure
        var portrait = portraitData.Value as SaveObject;
        Assert.IsNotNull(portrait);
        var portraitProps = portrait.Properties.ToList();
        Assert.IsTrue(portraitProps.Any());

        // Output portrait data for debugging
        foreach (var prop in portraitProps)
        {
            TestContext.WriteLine($"{prop.Key} = {prop.Value}");
        }
    }

    [TestMethod]
    public void TestParseAchievements()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "gamestate-achievement")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Verify root structure
        var root = document.Root as SaveObject;
        Assert.IsNotNull(root);
        var achievementData = root.Properties.Single();
        Assert.AreEqual("achievement", achievementData.Key);

        // Verify achievement data structure - it should be an array of achievement IDs
        var achievementArray = achievementData.Value as SaveArray;
        Assert.IsNotNull(achievementArray, "Achievement data should be an array");
        Assert.IsTrue(achievementArray.Items.Any(), "Achievement array should not be empty");

        // Output achievement IDs for debugging
        foreach (var achievement in achievementArray.Items)
        {
            TestContext.WriteLine($"Achievement ID: {achievement}");
        }
    }
}