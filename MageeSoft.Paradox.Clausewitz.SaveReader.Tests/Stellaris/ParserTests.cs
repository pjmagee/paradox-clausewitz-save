using Microsoft.VisualStudio.TestTools.UnitTesting;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Globalization;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Parse_SimpleArray_ReturnsArrayValues()
    {
        // Arrange
        string input = "test={ 1 2 3 4 5 }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var testProp = root.Properties[0];
        Assert.AreEqual("test", testProp.Key, "Property key should be 'test'");
        
        Assert.IsInstanceOfType(testProp.Value, typeof(SaveArray));
        var array = (SaveArray)testProp.Value;
        
        Assert.AreEqual(5, array.Items.Length, "Array should have 5 items");
        
        // Check each item is an integer with the expected value
        for (int i = 0; i < 5; i++)
        {
            Assert.IsTrue(array.Items[i].TryGetScalar<int>(out var value), $"Item {i} should be an integer");
            Assert.AreEqual(i + 1, value, $"Item {i} should have value {i + 1}");
        }
    }

    [TestMethod]
    public void Parse_AchievementArray_ReturnsArrayValues()
    {
        // Arrange
        string input = @"achievement={
            22 27 30 37 40
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var achievementProp = root.Properties[0];
        Assert.AreEqual("achievement", achievementProp.Key, "Property key should be 'achievement'");
        
        Assert.IsInstanceOfType(achievementProp.Value, typeof(SaveArray));
        var array = (SaveArray)achievementProp.Value;
        
        Assert.AreEqual(5, array.Items.Length, "Array should have 5 items");
        
        // Expected values
        int[] expectedValues = { 22, 27, 30, 37, 40 };
        
        // Check each item is an integer with the expected value
        for (int i = 0; i < expectedValues.Length; i++)
        {
            Assert.IsTrue(array.Items[i].TryGetScalar<int>(out var value), $"Item {i} should be an integer");
            Assert.AreEqual(expectedValues[i], value, $"Item {i} should have value {expectedValues[i]}");
        }
    }

    [TestMethod]
    public void Parse_NestedObjects_ReturnsNestedStructure()
    {
        // Arrange
        string input = @"
        country={
            name=""Test Empire""
            capital=5
            resources={
                energy=100
                minerals=200
            }
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var countryProp = root.Properties[0];
        Assert.AreEqual("country", countryProp.Key, "Property key should be 'country'");
        
        Assert.IsInstanceOfType(countryProp.Value, typeof(SaveObject));
        var country = (SaveObject)countryProp.Value;
        
        // Check country properties
        Assert.AreEqual(3, country.Properties.Length, "Country should have 3 properties");
        
        // Check name
        var nameProp = country.Properties.First(p => p.Key == "name");
        Assert.IsTrue(nameProp.Value.TryGetScalar<string>(out var nameValue));
        Assert.AreEqual("Test Empire", nameValue);
        
        // Check capital
        var capitalProp = country.Properties.First(p => p.Key == "capital");
        Assert.IsTrue(capitalProp.Value.TryGetScalar<int>(out var capitalValue));
        Assert.AreEqual(5, capitalValue);
        
        // Check resources
        var resourcesProp = country.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(SaveObject));
        var resources = (SaveObject)resourcesProp.Value;
        
        // Check energy
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsTrue(energyProp.Value.TryGetScalar<int>(out var energyValue));
        Assert.AreEqual(100, energyValue);
        
        // Check minerals
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsTrue(mineralsProp.Value.TryGetScalar<int>(out var mineralsValue));
        Assert.AreEqual(200, mineralsValue);
    }

    [TestMethod]
    public void Parse_BooleanValues_ReturnsBooleanScalars()
    {
        // Arrange
        string input = @"
        settings={
            ironman=yes
            multiplayer=no
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var settingsProp = root.Properties[0];
        Assert.AreEqual("settings", settingsProp.Key, "Property key should be 'settings'");
        
        Assert.IsInstanceOfType(settingsProp.Value, typeof(SaveObject));
        var settings = (SaveObject)settingsProp.Value;
        
        // Check ironman (yes)
        var ironmanProp = settings.Properties.First(p => p.Key == "ironman");
        Assert.IsTrue(ironmanProp.Value.TryGetScalar<bool>(out var ironmanValue));
        Assert.IsTrue(ironmanValue);
        
        // Check multiplayer (no)
        var multiplayerProp = settings.Properties.First(p => p.Key == "multiplayer");
        Assert.IsTrue(multiplayerProp.Value.TryGetScalar<bool>(out var multiplayerValue));
        Assert.IsFalse(multiplayerValue);
    }

    [TestMethod]
    public void Parse_DateValues_ReturnsDateScalars()
    {
        // Arrange
        string input = @"
        game={
            start_date=""2200.01.01""
            current_date=""2250.05.12""
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var gameProp = root.Properties[0];
        Assert.AreEqual("game", gameProp.Key, "Property key should be 'game'");
        
        Assert.IsInstanceOfType(gameProp.Value, typeof(SaveObject));
        var game = (SaveObject)gameProp.Value;
        
        // Check start_date
        var startDateProp = game.Properties.First(p => p.Key == "start_date");
        Assert.IsTrue(startDateProp.Value.TryGetScalar<DateOnly>(out var startDateValue));
        Assert.AreEqual(new DateOnly(2200, 1, 1), startDateValue);
        
        // Check current_date
        var currentDateProp = game.Properties.First(p => p.Key == "current_date");
        Assert.IsTrue(currentDateProp.Value.TryGetScalar<DateOnly>(out var currentDateValue));
        Assert.AreEqual(new DateOnly(2250, 5, 12), currentDateValue);
    }

    [TestMethod]
    public void Parse_NumericValues_ReturnsCorrectScalarTypes()
    {
        // Arrange
        string input = @"
        values={
            integer=42
            float=3.14
            large_integer=9223372036854775807
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var valuesProp = root.Properties[0];
        Assert.AreEqual("values", valuesProp.Key, "Property key should be 'values'");
        
        Assert.IsInstanceOfType(valuesProp.Value, typeof(SaveObject));
        var values = (SaveObject)valuesProp.Value;
        
        // Check integer
        var integerProp = values.Properties.First(p => p.Key == "integer");
        Assert.IsTrue(integerProp.Value.TryGetScalar<int>(out var integerValue));
        Assert.AreEqual(42, integerValue);
        
        // Check float
        var floatProp = values.Properties.First(p => p.Key == "float");
        Assert.IsTrue(floatProp.Value.TryGetScalar<float>(out var floatValue));
        Assert.AreEqual(3.14f, floatValue, 0.0001f);
        
        // Check large integer
        var largeIntegerProp = values.Properties.First(p => p.Key == "large_integer");
        Assert.IsTrue(largeIntegerProp.Value.TryGetScalar<long>(out var largeIntegerValue));
        Assert.AreEqual(9223372036854775807, largeIntegerValue);
    }

    [TestMethod]
    public void Parse_EmptyBlock_ReturnsEmptyObject()
    {
        // Arrange
        string input = "empty={}";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var emptyProp = root.Properties[0];
        Assert.AreEqual("empty", emptyProp.Key, "Property key should be 'empty'");
        
        Assert.IsInstanceOfType(emptyProp.Value, typeof(SaveObject));
        var empty = (SaveObject)emptyProp.Value;
        
        Assert.AreEqual(0, empty.Properties.Length, "Empty object should have 0 properties");
    }
    
    [TestMethod]
    public void Parse_GuidValues_ReturnsGuidScalars()
    {
        // Arrange
        string input = @"
        guids={
            id=""00000000-0000-0000-0000-000000000000""
            random_id=""12345678-1234-5678-1234-567812345678""
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var guidsProp = root.Properties[0];
        Assert.AreEqual("guids", guidsProp.Key, "Property key should be 'guids'");
        
        Assert.IsInstanceOfType(guidsProp.Value, typeof(SaveObject));
        var guids = (SaveObject)guidsProp.Value;
        
        // Check empty GUID
        var idProp = guids.Properties.First(p => p.Key == "id");
        Assert.IsTrue(idProp.Value.TryGetScalar<Guid>(out var idValue));
        Assert.AreEqual(Guid.Empty, idValue);
        
        // Check random GUID
        var randomIdProp = guids.Properties.First(p => p.Key == "random_id");
        Assert.IsTrue(randomIdProp.Value.TryGetScalar<Guid>(out var randomIdValue));
        Assert.AreEqual(new Guid("12345678-1234-5678-1234-567812345678"), randomIdValue);
    }
    
    [TestMethod]
    public void Parse_NestedArrays_ReturnsCorrectStructure()
    {
        // Arrange
        string input = @"
        nested_arrays={
            simple_array={ 1 2 3 }
            complex_array={
                { name=""Item 1"" value=10 }
                { name=""Item 2"" value=20 }
            }
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var nestedArraysProp = root.Properties[0];
        Assert.AreEqual("nested_arrays", nestedArraysProp.Key, "Property key should be 'nested_arrays'");
        
        Assert.IsInstanceOfType(nestedArraysProp.Value, typeof(SaveObject));
        var nestedArrays = (SaveObject)nestedArraysProp.Value;
        
        // Check simple array
        var simpleArrayProp = nestedArrays.Properties.First(p => p.Key == "simple_array");
        Assert.IsInstanceOfType(simpleArrayProp.Value, typeof(SaveArray));
        var simpleArray = (SaveArray)simpleArrayProp.Value;
        
        Assert.AreEqual(3, simpleArray.Items.Length, "Simple array should have 3 items");
        for (int i = 0; i < 3; i++)
        {
            Assert.IsTrue(simpleArray.Items[i].TryGetScalar<int>(out var value));
            Assert.AreEqual(i + 1, value);
        }
        
        // Check complex array
        var complexArrayProp = nestedArrays.Properties.First(p => p.Key == "complex_array");
        Assert.IsInstanceOfType(complexArrayProp.Value, typeof(SaveArray));
        var complexArray = (SaveArray)complexArrayProp.Value;
        
        Assert.AreEqual(2, complexArray.Items.Length, "Complex array should have 2 items");
        
        // Check first item
        Assert.IsInstanceOfType(complexArray.Items[0], typeof(SaveObject));
        var item1 = (SaveObject)complexArray.Items[0];
        
        var item1NameProp = item1.Properties.First(p => p.Key == "name");
        Assert.IsTrue(item1NameProp.Value.TryGetScalar<string>(out var item1Name));
        Assert.AreEqual("Item 1", item1Name);
        
        var item1ValueProp = item1.Properties.First(p => p.Key == "value");
        Assert.IsTrue(item1ValueProp.Value.TryGetScalar<int>(out var item1Value));
        Assert.AreEqual(10, item1Value);
        
        // Check second item
        Assert.IsInstanceOfType(complexArray.Items[1], typeof(SaveObject));
        var item2 = (SaveObject)complexArray.Items[1];
        
        var item2NameProp = item2.Properties.First(p => p.Key == "name");
        Assert.IsTrue(item2NameProp.Value.TryGetScalar<string>(out var item2Name));
        Assert.AreEqual("Item 2", item2Name);
        
        var item2ValueProp = item2.Properties.First(p => p.Key == "value");
        Assert.IsTrue(item2ValueProp.Value.TryGetScalar<int>(out var item2Value));
        Assert.AreEqual(20, item2Value);
    }
    
    [TestMethod]
    public void Parse_DeepNestedStructure_ReturnsCorrectHierarchy()
    {
        // Arrange
        string input = @"
        galaxy={
            planets={
                1={
                    name=""Earth""
                    size=10
                    moons={
                        1={ name=""Moon"" size=2 }
                    }
                    resources={
                        energy=100
                        minerals={
                            base=50
                            bonus=25
                        }
                    }
                }
            }
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        // Check galaxy
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var galaxyProp = root.Properties[0];
        Assert.AreEqual("galaxy", galaxyProp.Key, "Property key should be 'galaxy'");
        Assert.IsInstanceOfType(galaxyProp.Value, typeof(SaveObject));
        var galaxy = (SaveObject)galaxyProp.Value;
        
        // Check planets
        var planetsProp = galaxy.Properties.First(p => p.Key == "planets");
        Assert.IsInstanceOfType(planetsProp.Value, typeof(SaveObject));
        var planets = (SaveObject)planetsProp.Value;
        
        // Check planet 1
        var planet1Prop = planets.Properties.First(p => p.Key == "1");
        Assert.IsInstanceOfType(planet1Prop.Value, typeof(SaveObject));
        var planet1 = (SaveObject)planet1Prop.Value;
        
        // Check planet name
        var planetNameProp = planet1.Properties.First(p => p.Key == "name");
        Assert.IsTrue(planetNameProp.Value.TryGetScalar<string>(out var planetName));
        Assert.AreEqual("Earth", planetName);
        
        // Check planet size
        var planetSizeProp = planet1.Properties.First(p => p.Key == "size");
        Assert.IsTrue(planetSizeProp.Value.TryGetScalar<int>(out var planetSize));
        Assert.AreEqual(10, planetSize);
        
        // Check moons
        var moonsProp = planet1.Properties.First(p => p.Key == "moons");
        Assert.IsInstanceOfType(moonsProp.Value, typeof(SaveObject));
        var moons = (SaveObject)moonsProp.Value;
        
        // Check moon 1
        var moon1Prop = moons.Properties.First(p => p.Key == "1");
        Assert.IsInstanceOfType(moon1Prop.Value, typeof(SaveObject));
        var moon1 = (SaveObject)moon1Prop.Value;
        
        // Check moon name
        var moonNameProp = moon1.Properties.First(p => p.Key == "name");
        Assert.IsTrue(moonNameProp.Value.TryGetScalar<string>(out var moonName));
        Assert.AreEqual("Moon", moonName);
        
        // Check moon size
        var moonSizeProp = moon1.Properties.First(p => p.Key == "size");
        Assert.IsTrue(moonSizeProp.Value.TryGetScalar<int>(out var moonSize));
        Assert.AreEqual(2, moonSize);
        
        // Check resources
        var resourcesProp = planet1.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(SaveObject));
        var resources = (SaveObject)resourcesProp.Value;
        
        // Check energy
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsTrue(energyProp.Value.TryGetScalar<int>(out var energy));
        Assert.AreEqual(100, energy);
        
        // Check minerals
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(SaveObject));
        var minerals = (SaveObject)mineralsProp.Value;
        
        // Check minerals base
        var mineralsBaseProp = minerals.Properties.First(p => p.Key == "base");
        Assert.IsTrue(mineralsBaseProp.Value.TryGetScalar<int>(out var mineralsBase));
        Assert.AreEqual(50, mineralsBase);
        
        // Check minerals bonus
        var mineralsBonusProp = minerals.Properties.First(p => p.Key == "bonus");
        Assert.IsTrue(mineralsBonusProp.Value.TryGetScalar<int>(out var mineralsBonus));
        Assert.AreEqual(25, mineralsBonus);
    }
    
    [TestMethod]
    public void Parse_MultipleTopLevelProperties_ReturnsAllProperties()
    {
        // Arrange
        string input = @"
        name=""Test Empire""
        capital=5
        resources={ energy=100 minerals=200 }
        flags={ is_xenophile=yes is_pacifist=no }
        ";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(4, root.Properties.Length, "Root should have 4 properties");
        
        // Check name
        var nameProp = root.Properties.First(p => p.Key == "name");
        Assert.IsTrue(nameProp.Value.TryGetScalar<string>(out var name));
        Assert.AreEqual("Test Empire", name);
        
        // Check capital
        var capitalProp = root.Properties.First(p => p.Key == "capital");
        Assert.IsTrue(capitalProp.Value.TryGetScalar<int>(out var capital));
        Assert.AreEqual(5, capital);
        
        // Check resources
        var resourcesProp = root.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(SaveObject));
        var resources = (SaveObject)resourcesProp.Value;
        
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsTrue(energyProp.Value.TryGetScalar<int>(out var energy));
        Assert.AreEqual(100, energy);
        
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsTrue(mineralsProp.Value.TryGetScalar<int>(out var minerals));
        Assert.AreEqual(200, minerals);
        
        // Check flags
        var flagsProp = root.Properties.First(p => p.Key == "flags");
        Assert.IsInstanceOfType(flagsProp.Value, typeof(SaveObject));
        var flags = (SaveObject)flagsProp.Value;
        
        var xenophileProp = flags.Properties.First(p => p.Key == "is_xenophile");
        Assert.IsTrue(xenophileProp.Value.TryGetScalar<bool>(out var isXenophile));
        Assert.IsTrue(isXenophile);
        
        var pacifistProp = flags.Properties.First(p => p.Key == "is_pacifist");
        Assert.IsTrue(pacifistProp.Value.TryGetScalar<bool>(out var isPacifist));
        Assert.IsFalse(isPacifist);
    }

    [TestMethod]
    public void Parse_MixedArrayAndProperties_ReturnsCorrectStructure()
    {
        // Arrange
        string input = @"
        planets={
            1={
                name=""Earth""
                size=10
            }
            2={
                name=""Mars""
                size=8
            }
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var planetsProp = root.Properties[0];
        Assert.AreEqual("planets", planetsProp.Key, "Property key should be 'planets'");
        
        Assert.IsInstanceOfType(planetsProp.Value, typeof(SaveObject));
        var planets = (SaveObject)planetsProp.Value;
        
        // Should have 2 planets with numeric keys
        Assert.AreEqual(2, planets.Properties.Length, "Planets should have 2 properties");
        
        // Check planet 1
        var planet1Prop = planets.Properties.First(p => p.Key == "1");
        Assert.IsInstanceOfType(planet1Prop.Value, typeof(SaveObject));
        var planet1 = (SaveObject)planet1Prop.Value;
        
        var planet1NameProp = planet1.Properties.First(p => p.Key == "name");
        Assert.IsTrue(planet1NameProp.Value.TryGetScalar<string>(out var planet1Name));
        Assert.AreEqual("Earth", planet1Name);
        
        var planet1SizeProp = planet1.Properties.First(p => p.Key == "size");
        Assert.IsTrue(planet1SizeProp.Value.TryGetScalar<int>(out var planet1Size));
        Assert.AreEqual(10, planet1Size);
        
        // Check planet 2
        var planet2Prop = planets.Properties.First(p => p.Key == "2");
        Assert.IsInstanceOfType(planet2Prop.Value, typeof(SaveObject));
        var planet2 = (SaveObject)planet2Prop.Value;
        
        var planet2NameProp = planet2.Properties.First(p => p.Key == "name");
        Assert.IsTrue(planet2NameProp.Value.TryGetScalar<string>(out var planet2Name));
        Assert.AreEqual("Mars", planet2Name);
        
        var planet2SizeProp = planet2.Properties.First(p => p.Key == "size");
        Assert.IsTrue(planet2SizeProp.Value.TryGetScalar<int>(out var planet2Size));
        Assert.AreEqual(8, planet2Size);
    }
} 