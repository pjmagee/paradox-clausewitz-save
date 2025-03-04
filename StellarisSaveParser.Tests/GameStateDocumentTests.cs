using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace StellarisSaveParser.Tests;

public class GameStateDocumentTests
{
    private readonly ITestOutputHelper _output;

    public GameStateDocumentTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(Skip = "Large File")]
    public void TestParse()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate"));
        Assert.NotNull(document);

        var root = document.RootElement;
        Assert.NotNull(root);

        foreach (Element? element in document.RootElement.EnumerateElements())
        {
            _output.WriteLine(element.ToString());
        }

        var galaxyName = document.RootElement
            .EnumerateObject()
            .Where(p => p.Key == "galaxy")
            .Single()
            .Value.EnumerateObject()
            .Where(p => p.Key == "name")
            .Single()
            .Value;

        _output.WriteLine(galaxyName.ToString());
    }

    [Fact]
    public void TestParseMarketFile()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-market"));
        Assert.NotNull(document);
        Assert.NotNull(document.RootElement);

        // Verify root structure
        var root = document.RootElement;
        var marketData = root.EnumerateObject().Single();
        Assert.Equal("market", marketData.Key);

        // Verify market object structure
        var market = marketData.Value;
        var marketProps = market.EnumerateObject().ToList();
        Assert.Contains(marketProps, p => p.Key == "id");
        Assert.Contains(marketProps, p => p.Key == "resources_bought");
        Assert.Contains(marketProps, p => p.Key == "resources_sold");
        Assert.Contains(marketProps, p => p.Key == "internal_market_fluctuations");

        // Test array parsing
        var idArray = marketProps.First(p => p.Key == "id").Value.EnumerateArray().ToList();
        Assert.True(idArray.Count > 0);

        // Test nested object structure
        var resourcesBought = marketProps.First(p => p.Key == "resources_bought").Value;
        var countries = resourcesBought.EnumerateObject().ToList();
        Assert.True(countries.Count > 0);

        // Verify country data structure
        var firstCountry = countries.First();
        Assert.Equal("country", firstCountry.Key);
        var countryAmount = countries[1];
        Assert.Equal("amount", countryAmount.Key);
    }

    [Fact]
    public void TestParseSpeciesDb()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-species_db"));
        Assert.NotNull(document);

        var root = document.RootElement;
        var speciesDb = root.EnumerateObject().Single();
        Assert.Equal("species_db", speciesDb.Key);

        // Test nested object traversal
        var dbContents = speciesDb.Value.EnumerateObject().ToList();
        Assert.True(dbContents.Count > 0);
    }

    [Fact]
    public void TestParseEventTargets()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-saved_event_target"));
        Assert.NotNull(document);

        var root = document.RootElement;
        var eventTarget = root.EnumerateObject().Single();
        Assert.Equal("saved_event_target", eventTarget.Key);

        // Test object structure
        var targetData = eventTarget.Value.EnumerateObject().ToList();
        Assert.Contains(targetData, p => p.Key == "type");
        Assert.Contains(targetData, p => p.Key == "id");
        Assert.Contains(targetData, p => p.Key == "name");
        
        // Verify type is country
        var type = targetData.First(p => p.Key == "type").Value;
        Assert.Contains("country", type.ToString());

        // Verify ID is numeric
        var id = targetData.First(p => p.Key == "id").Value;
        Assert.True(ulong.TryParse(id.ToString().Replace("Scalar: ", ""), out _));
    }

    [Fact]
    public void TestParseStorms()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-storms"));
        Assert.NotNull(document);

        // Verify root structure
        var root = document.RootElement;
        var storms = root.EnumerateObject().Single();
        Assert.Equal("storms", storms.Key);

        // Get storms collection
        var stormsCollection = storms.Value.EnumerateObject().First(p => p.Key == "storms").Value;
        var stormEntries = stormsCollection.EnumerateObject().ToList();
        Assert.True(stormEntries.Count > 0);

        // Find a valid storm entry (skip "none" entries)
        var stormEntry = stormEntries.First(s => s.Value.ToString() != "Scalar: none");
        var stormProps = stormEntry.Value.EnumerateObject().ToList();

        // Verify required storm properties
        Assert.Contains(stormProps, p => p.Key == "type");
        Assert.Contains(stormProps, p => p.Key == "cosmic_storm_start_position");
        Assert.Contains(stormProps, p => p.Key == "storm_current_pos");
        Assert.Contains(stormProps, p => p.Key == "storm_target_pos");
        Assert.Contains(stormProps, p => p.Key == "color");
        Assert.Contains(stormProps, p => p.Key == "cluster");
        Assert.Contains(stormProps, p => p.Key == "name");
        Assert.Contains(stormProps, p => p.Key == "prefix");
        Assert.Contains(stormProps, p => p.Key == "path");

        // Test position structure
        var currentPos = stormProps.First(p => p.Key == "storm_current_pos").Value;
        var posProps = currentPos.EnumerateObject().ToList();
        Assert.Contains(posProps, p => p.Key == "x");
        Assert.Contains(posProps, p => p.Key == "y");
        Assert.Contains(posProps, p => p.Key == "origin");
        Assert.Contains(posProps, p => p.Key == "randomized");

        // Verify coordinates are decimal numbers
        var xCoord = posProps.First(p => p.Key == "x").Value.ToString().Replace("Scalar: ", "");
        var yCoord = posProps.First(p => p.Key == "y").Value.ToString().Replace("Scalar: ", "");
        Assert.True(decimal.TryParse(xCoord, out _), $"X coordinate {xCoord} is not a valid decimal");
        Assert.True(decimal.TryParse(yCoord, out _), $"Y coordinate {yCoord} is not a valid decimal");

        // Test color structure (RGBA values)
        var color = stormProps.First(p => p.Key == "color").Value;
        var colorValues = color.EnumerateArray().ToList();
        Assert.Equal(4, colorValues.Count); // RGBA
        foreach (var component in colorValues)
        {
            var value = decimal.Parse(component.ToString().Replace("Scalar: ", ""));
            Assert.True(value >= 0 && value <= 1, $"Color component {value} is not between 0 and 1");
        }

        // Test cluster structure
        var cluster = stormProps.First(p => p.Key == "cluster").Value;
        var clusterProps = cluster.EnumerateObject().ToList();
        Assert.Contains(clusterProps, p => p.Key == "id");
        Assert.Contains(clusterProps, p => p.Key == "position");
        Assert.Contains(clusterProps, p => p.Key == "radius");
        Assert.Contains(clusterProps, p => p.Key == "objects");

        // Verify cluster objects are integers
        var objects = clusterProps.First(p => p.Key == "objects").Value.EnumerateArray().ToList();
        Assert.True(objects.Count > 0);
        foreach (var obj in objects)
        {
            Assert.True(int.TryParse(obj.ToString().Replace("Scalar: ", ""), out _));
        }

        // Test path structure
        var path = stormProps.First(p => p.Key == "path").Value.EnumerateArray().ToList();
        Assert.True(path.Count > 0);
        foreach (var pathPoint in path)
        {
            Assert.True(int.TryParse(pathPoint.ToString().Replace("Scalar: ", ""), out _));
        }

        // Verify storm name and prefix structure
        var name = stormProps.First(p => p.Key == "name").Value;
        var nameProps = name.EnumerateObject().ToList();
        Assert.Contains(nameProps, p => p.Key == "key");
        Assert.Contains("ART2_CHR_", nameProps.First(p => p.Key == "key").Value.ToString());

        // Verify numeric properties
        Assert.True(int.TryParse(stormProps.First(p => p.Key == "storm_age").Value.ToString().Replace("Scalar: ", ""), out _));
        Assert.True(decimal.TryParse(stormProps.First(p => p.Key == "speed").Value.ToString().Replace("Scalar: ", ""), out _));
        Assert.True(int.TryParse(stormProps.First(p => p.Key == "length").Value.ToString().Replace("Scalar: ", ""), out _));

        // Verify cosmic storm name list
        var nameListName = storms.Value.EnumerateObject()
            .First(p => p.Key == "cosmic_storm_name_list_name")
            .Value;
        Assert.Equal("ART2", nameListName.ToString().Replace("Scalar: ", ""));
    }

    [Fact]
    public void TestParseMarketFluctuations()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-market"));
        Assert.NotNull(document);

        // Navigate to internal_market_fluctuations
        var fluctuations = document.RootElement
            .EnumerateObject()
            .Single()
            .Value
            .EnumerateObject()
            .First(p => p.Key == "internal_market_fluctuations")
            .Value;

        // Test country resource fluctuations
        var countries = fluctuations.EnumerateObject().ToList();
        Assert.True(countries.Count > 0);

        // Test resources structure
        var firstCountry = countries.First();
        Assert.Equal("country", firstCountry.Key);
        var resources = countries[1];
        Assert.Equal("resources", resources.Key);
    }

    [Fact]
    public void TestParseResourceAmounts()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-market"));
        Assert.NotNull(document);

        // Navigate to resources_bought
        var resourcesBought = document.RootElement
            .EnumerateObject()
            .Single()
            .Value
            .EnumerateObject()
            .First(p => p.Key == "resources_bought")
            .Value;

        // Test country resource amounts
        var entries = resourcesBought.EnumerateObject().ToList();
        Assert.True(entries.Count > 0);

        // Verify structure alternates between country and amount
        for (int i = 0; i < entries.Count - 1; i += 2)
        {
            Assert.Equal("country", entries[i].Key);
            Assert.Equal("amount", entries[i + 1].Key);

            // Test amount array structure
            var amounts = entries[i + 1].Value.EnumerateArray().ToList();
            Assert.True(amounts.Count > 0);
        }
    }

    [Fact]
    public void TestParseSpeciesPortrait()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-used_species_portrait"));
        Assert.NotNull(document);

        // Verify root structure
        var root = document.RootElement;
        var portraitData = root.EnumerateObject().Single();
        Assert.Equal("used_species_portrait", portraitData.Key);

        // Get portrait properties
        var portrait = portraitData.Value;
        var portraitProps = portrait.EnumerateObject().ToList();

        // Verify required properties
        Assert.Contains(portraitProps, p => p.Key == "class");
        Assert.Contains(portraitProps, p => p.Key == "values");

        // Test class value
        var classValue = portraitProps.First(p => p.Key == "class").Value;
        Assert.Contains("HUM", classValue.ToString());

        // Test values array
        var values = portraitProps.First(p => p.Key == "values").Value;
        var valuesList = values.EnumerateArray().ToList();
        Assert.True(valuesList.Count > 0);

        // Verify values are numeric
        foreach (var value in valuesList)
        {
            Assert.True(long.TryParse(value.ToString().Replace("Scalar: ", ""), out _));
        }
    }

    [Fact]
    public void TestParseAchievements()
    {
        var document = GameStateDocument.Parse(new FileInfo("./TestData/gamestate-achievement"));
        Assert.NotNull(document);

        // Verify root structure
        var root = document.RootElement;
        var achievementData = root.EnumerateObject().Single();
        Assert.Equal("achievement", achievementData.Key);

        // Get achievement values
        var achievements = achievementData.Value.EnumerateArray().ToList();
        Assert.True(achievements.Count > 0);

        // Verify all values are integers and in ascending order
        int? previousValue = null;
        foreach (var achievement in achievements)
        {
            var valueStr = achievement.ToString().Replace("Scalar: ", "");
            Assert.True(int.TryParse(valueStr, out int value), $"Value {valueStr} is not a valid integer");
            
            if (previousValue.HasValue)
            {
                Assert.True(value > previousValue.Value, $"Achievement IDs should be in ascending order. Found {value} after {previousValue.Value}");
            }
            previousValue = value;
        }

        // Verify some known achievement IDs are present
        var achievementIds = achievements.Select(a => int.Parse(a.ToString().Replace("Scalar: ", ""))).ToList();
        Assert.Contains(22, achievementIds); // First ID in the file
        Assert.Contains(191, achievementIds); // Last ID in the file
        Assert.Contains(100, achievementIds); // Middle ID
    }
}