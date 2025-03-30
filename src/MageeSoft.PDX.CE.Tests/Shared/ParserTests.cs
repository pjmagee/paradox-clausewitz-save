namespace MageeSoft.PDX.CE.Tests.Shared;

[TestClass]
public class ParserTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Parse_ListOfStringKeyValues_ReturnsCorrectKeyValuePairs()
    {
        // Arrange
        string input = """
ship_names=
{
	"REP3_SHIP_Erid-Sur"=1
	"%SEQ%"=14
	"REP3_SHIP_Lorod-Gexad"=1
}
""";
        // Act
        var root = Parser.Parse(input);
        
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        var rootObj = (SaveObject)root;
        Assert.AreEqual(1, rootObj.Properties.Count, "Root should have exactly one property");
        
        var shipNamesProp = rootObj.Properties[0];
        Assert.AreEqual("ship_names", shipNamesProp.Key, "Property key should be 'ship_names'");
        Assert.IsInstanceOfType(shipNamesProp.Value, typeof(SaveObject));
        
        var shipNames = (SaveObject)shipNamesProp.Value;
        Assert.AreEqual(3, shipNames.Properties.Count, "Ship names should have 3 properties");
        Assert.IsInstanceOfType(shipNames.Properties[0].Value, typeof(Scalar<int>));
        
        var firstShipName = shipNames.Properties[0];
        Assert.AreEqual("REP3_SHIP_Erid-Sur", firstShipName.Key, "First ship name key should be 'REP3_SHIP_Erid-Sur'");
        Assert.AreEqual(1, ((Scalar<int>)firstShipName.Value).Value, "First ship name value should be 1");
        Assert.IsInstanceOfType(shipNames.Properties[1].Value, typeof(Scalar<int>));
        
        var secondShipName = shipNames.Properties[1];
        Assert.AreEqual("%SEQ%", secondShipName.Key, "Second ship name key should be '%SEQ%'");
        Assert.AreEqual(14, ((Scalar<int>)secondShipName.Value).Value, "Second ship name value should be 14");
        Assert.IsInstanceOfType(shipNames.Properties[2].Value, typeof(Scalar<int>));
        
        var thirdShipName = shipNames.Properties[2];
        Assert.AreEqual("REP3_SHIP_Lorod-Gexad", thirdShipName.Key, "Third ship name key should be 'REP3_SHIP_Lorod-Gexad'");
        Assert.AreEqual(1, ((Scalar<int>)thirdShipName.Value).Value, "Third ship name value should be 1");
        Assert.IsInstanceOfType(shipNames.Properties[2].Value, typeof(Scalar<int>));
    }
    
    [TestMethod]
    public void Parse_SimpleArray_ReturnsArrayValues()
    {
        // Arrange
        int[] expectedValues = { 22, 27, 30, 37, 40 };
        string input = "test={" + string.Join(" ", expectedValues) + "}";
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var testProp = root.Properties[0];
        Assert.AreEqual("test", testProp.Key, "Property key should be 'test'");
        
        Assert.IsInstanceOfType(testProp.Value, typeof(SaveArray));
        var array = (SaveArray)testProp.Value;
        
        Assert.AreEqual(expectedValues.Length, array.Items.Count, "Array should have same number of items");
       
        for (int i = 0; i < expectedValues.Length; i++)
        {
            Assert.IsInstanceOfType(array.Items[i], typeof(Scalar<int>));
            var scalar = (Scalar<int>)array.Items[i];
            Assert.AreEqual(expectedValues[i], scalar.Value, $"Item {i} should have value {i + 1}");
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
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
                
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var countryProp = root.Properties[0];
        Assert.AreEqual("country", countryProp.Key, "Property key should be 'country'");
        
        Assert.IsInstanceOfType(countryProp.Value, typeof(SaveObject));
        var country = (SaveObject)countryProp.Value;
        
        // Check country properties
        Assert.AreEqual(3, country.Properties.Count, "Country should have 3 properties");
        
        // Check name
        var nameProp = country.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(nameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Test Empire", ((Scalar<string>)nameProp.Value).Value);
        
        // Check capital
        var capitalProp = country.Properties.First(p => p.Key == "capital");
        Assert.IsInstanceOfType(capitalProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(5, ((Scalar<int>)capitalProp.Value).Value);
        
        // Check resources
        var resourcesProp = country.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(SaveObject));
        var resources = (SaveObject)resourcesProp.Value;
        
        // Check energy
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(100, ((Scalar<int>)energyProp.Value).Value);
        
        // Check minerals
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(200, ((Scalar<int>)mineralsProp.Value).Value);
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

        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var settingsProp = root.Properties[0];
        Assert.AreEqual("settings", settingsProp.Key, "Property key should be 'settings'");
        
        Assert.IsInstanceOfType(settingsProp.Value, typeof(SaveObject));
        var settings = (SaveObject)settingsProp.Value;
        
        // Check ironman (yes)
        var ironmanProp = settings.Properties.First(p => p.Key == "ironman");
        Assert.IsInstanceOfType(ironmanProp.Value, typeof(Scalar<bool>));
        Assert.IsTrue(((Scalar<bool>)ironmanProp.Value).Value);
        
        // Check multiplayer (no)
        var multiplayerProp = settings.Properties.First(p => p.Key == "multiplayer");
        Assert.IsInstanceOfType(multiplayerProp.Value, typeof(Scalar<bool>));
        Assert.IsFalse(((Scalar<bool>)multiplayerProp.Value).Value);
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

        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var gameProp = root.Properties[0];
        Assert.AreEqual("game", gameProp.Key, "Property key should be 'game'");
        
        Assert.IsInstanceOfType(gameProp.Value, typeof(SaveObject));
        var game = (SaveObject)gameProp.Value;
        
        // Check start_date
        var startDateProp = game.Properties.First(p => p.Key == "start_date");
        Assert.IsInstanceOfType(startDateProp.Value, typeof(Scalar<DateOnly>));
        Assert.AreEqual(new DateOnly(2200, 1, 1), ((Scalar<DateOnly>)startDateProp.Value).Value);
        
        // Check current_date
        var currentDateProp = game.Properties.First(p => p.Key == "current_date");
        Assert.IsInstanceOfType(currentDateProp.Value, typeof(Scalar<DateOnly>));
        Assert.AreEqual(new DateOnly(2250, 5, 12), ((Scalar<DateOnly>)currentDateProp.Value).Value);
    }

    [TestMethod]
    public void Parse_MixedTypeArray_ReturnsCorrectScalarTypes()
    {
        // Arrange
        string input = @"
        mixed_array={
            42
            9223372036854775807
            3.14
        }";
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        var rootObj = (SaveObject)root;
        
        Assert.AreEqual(1, rootObj.Properties.Count, "Root should have exactly one property");
        var arrayProp = rootObj.Properties[0];
        Assert.AreEqual("mixed_array", arrayProp.Key, "Property key should be 'mixed_array'");
        
        Assert.IsInstanceOfType(arrayProp.Value, typeof(SaveArray));
        var mixedArray = (SaveArray)arrayProp.Value;
        
        Assert.AreEqual(3, mixedArray.Items.Count, "Array should have exactly three values");
        
        // Check integer value
        Assert.IsInstanceOfType(mixedArray.Items[0], typeof(Scalar<int>));
        Assert.AreEqual(42, ((Scalar<int>)mixedArray.Items[0]).Value);
        
        // Check long value
        Assert.IsInstanceOfType(mixedArray.Items[1], typeof(Scalar<long>));
        Assert.AreEqual(9223372036854775807, ((Scalar<long>)mixedArray.Items[1]).Value);
        
        // Check float value
        Assert.IsInstanceOfType(mixedArray.Items[2], typeof(Scalar<float>));
        Assert.AreEqual(3.14f, ((Scalar<float>)mixedArray.Items[2]).Value, 0.0001f);
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
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var valuesProp = root.Properties[0];
        Assert.AreEqual("values", valuesProp.Key, "Property key should be 'values'");
        
        Assert.IsInstanceOfType(valuesProp.Value, typeof(SaveObject));
        var values = (SaveObject)valuesProp.Value;
        
        // Check integer
        var integerProp = values.Properties.First(p => p.Key == "integer");
        Assert.IsInstanceOfType(integerProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(42, ((Scalar<int>)integerProp.Value).Value);
        
        // Check float
        var floatProp = values.Properties.First(p => p.Key == "float");
        Assert.IsInstanceOfType(floatProp.Value, typeof(Scalar<float>));
        Assert.AreEqual(3.14f, ((Scalar<float>)floatProp.Value).Value, 0.0001f);
        
        // Check large integer
        var largeIntegerProp = values.Properties.First(p => p.Key == "large_integer");
        Assert.IsInstanceOfType(largeIntegerProp.Value, typeof(Scalar<long>));
        Assert.AreEqual(9223372036854775807, ((Scalar<long>)largeIntegerProp.Value).Value);
    }

    [TestMethod]
    public void Parse_EmptyBlock_ReturnsEmptyObject()
    {
        // Arrange
        string input = "empty={}";
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var emptyProp = root.Properties[0];
        Assert.AreEqual("empty", emptyProp.Key, "Property key should be 'empty'");
        
        Assert.IsInstanceOfType(emptyProp.Value, typeof(SaveObject));
        var empty = (SaveObject)emptyProp.Value;
        
        Assert.AreEqual(0, empty.Properties.Count, "Empty object should have 0 properties");
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
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var guidsProp = root.Properties[0];
        Assert.AreEqual("guids", guidsProp.Key, "Property key should be 'guids'");
        
        Assert.IsInstanceOfType(guidsProp.Value, typeof(SaveObject));
        var guids = (SaveObject)guidsProp.Value;
        
        // Check empty GUID
        var idProp = guids.Properties.First(p => p.Key == "id");
        Assert.IsInstanceOfType(idProp.Value, typeof(Scalar<Guid>));
        Assert.AreEqual(Guid.Empty, ((Scalar<Guid>)idProp.Value).Value);
        
        // Check random GUID
        var randomIdProp = guids.Properties.First(p => p.Key == "random_id");
        Assert.IsInstanceOfType(randomIdProp.Value, typeof(Scalar<Guid>));
        Assert.AreEqual(new Guid("12345678-1234-5678-1234-567812345678"), ((Scalar<Guid>)randomIdProp.Value).Value);
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
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var nestedArraysProp = root.Properties[0];
        Assert.AreEqual("nested_arrays", nestedArraysProp.Key, "Property key should be 'nested_arrays'");
        
        Assert.IsInstanceOfType(nestedArraysProp.Value, typeof(SaveObject));
        var nestedArrays = (SaveObject)nestedArraysProp.Value;
        
        // Check simple array
        var simpleArrayProp = nestedArrays.Properties.First(p => p.Key == "simple_array");
        Assert.IsInstanceOfType(simpleArrayProp.Value, typeof(SaveArray));
        var simpleArray = (SaveArray)simpleArrayProp.Value;
        
        Assert.AreEqual(3, simpleArray.Items.Count, "Simple array should have 3 items");
        for (int i = 0; i < 3; i++)
        {
            Assert.IsInstanceOfType(simpleArray.Items[i], typeof(Scalar<int>));
            Assert.AreEqual(i + 1, ((Scalar<int>)simpleArray.Items[i]).Value);
        }
        
        // Check complex array
        var complexArrayProp = nestedArrays.Properties.First(p => p.Key == "complex_array");
        Assert.IsInstanceOfType(complexArrayProp.Value, typeof(SaveArray));
        var complexArray = (SaveArray)complexArrayProp.Value;
        
        Assert.AreEqual(2, complexArray.Items.Count, "Complex array should have 2 items");
        
        // Check first item
        Assert.IsInstanceOfType(complexArray.Items[0], typeof(SaveObject));
        var item1 = (SaveObject)complexArray.Items[0];
        
        var item1NameProp = item1.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(item1NameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Item 1", ((Scalar<string>)item1NameProp.Value).Value);
        
        var item1ValueProp = item1.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(item1ValueProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(10, ((Scalar<int>)item1ValueProp.Value).Value);
        
        // Check second item
        Assert.IsInstanceOfType(complexArray.Items[1], typeof(SaveObject));
        var item2 = (SaveObject)complexArray.Items[1];
        
        var item2NameProp = item2.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(item2NameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Item 2", ((Scalar<string>)item2NameProp.Value).Value);
        
        var item2ValueProp = item2.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(item2ValueProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(20, ((Scalar<int>)item2ValueProp.Value).Value);
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
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        
        
        // Check galaxy
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
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
        Assert.IsInstanceOfType(planetNameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Earth", ((Scalar<string>)planetNameProp.Value).Value);
        
        // Check planet size
        var planetSizeProp = planet1.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(planetSizeProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(10, ((Scalar<int>)planetSizeProp.Value).Value);
        
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
        Assert.IsInstanceOfType(moonNameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Moon", ((Scalar<string>)moonNameProp.Value).Value);
        
        // Check moon size
        var moonSizeProp = moon1.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(moonSizeProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(2, ((Scalar<int>)moonSizeProp.Value).Value);
        
        // Check resources
        var resourcesProp = planet1.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(SaveObject));
        var resources = (SaveObject)resourcesProp.Value;
        
        // Check energy
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(100, ((Scalar<int>)energyProp.Value).Value);
        
        // Check minerals
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(SaveObject));
        var minerals = (SaveObject)mineralsProp.Value;
        
        // Check minerals base
        var mineralsBaseProp = minerals.Properties.First(p => p.Key == "base");
        Assert.IsInstanceOfType(mineralsBaseProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(50, ((Scalar<int>)mineralsBaseProp.Value).Value);
        
        // Check minerals bonus
        var mineralsBonusProp = minerals.Properties.First(p => p.Key == "bonus");
        Assert.IsInstanceOfType(mineralsBonusProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(25, ((Scalar<int>)mineralsBonusProp.Value).Value);
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
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
                
        Assert.AreEqual(4, root.Properties.Count, "Root should have 4 properties");
        
        // Check name
        var nameProp = root.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(nameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Test Empire", ((Scalar<string>)nameProp.Value).Value);
        
        // Check capital
        var capitalProp = root.Properties.First(p => p.Key == "capital");
        Assert.IsInstanceOfType(capitalProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(5, ((Scalar<int>)capitalProp.Value).Value);
        
        // Check resources
        var resourcesProp = root.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(SaveObject));
        var resources = (SaveObject)resourcesProp.Value;
        
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(100, ((Scalar<int>)energyProp.Value).Value);
        
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(200, ((Scalar<int>)mineralsProp.Value).Value);
        
        // Check flags
        var flagsProp = root.Properties.First(p => p.Key == "flags");
        Assert.IsInstanceOfType(flagsProp.Value, typeof(SaveObject));
        var flags = (SaveObject)flagsProp.Value;
        
        var xenophileProp = flags.Properties.First(p => p.Key == "is_xenophile");
        Assert.IsInstanceOfType(xenophileProp.Value, typeof(Scalar<bool>));
        Assert.IsTrue(((Scalar<bool>)xenophileProp.Value).Value);
        
        var pacifistProp = flags.Properties.First(p => p.Key == "is_pacifist");
        Assert.IsInstanceOfType(pacifistProp.Value, typeof(Scalar<bool>));
        Assert.IsFalse(((Scalar<bool>)pacifistProp.Value).Value);
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
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var planetsProp = root.Properties[0];
        Assert.AreEqual("planets", planetsProp.Key, "Property key should be 'planets'");
        
        Assert.IsInstanceOfType(planetsProp.Value, typeof(SaveObject));
        var planets = (SaveObject)planetsProp.Value;
        
        // Should have 2 planets with numeric keys
        Assert.AreEqual(2, planets.Properties.Count, "Planets should have 2 properties");
        
        // Check planet 1
        var planet1Prop = planets.Properties.First(p => p.Key == "1");
        Assert.IsInstanceOfType(planet1Prop.Value, typeof(SaveObject));
        var planet1 = (SaveObject)planet1Prop.Value;
        
        var planet1NameProp = planet1.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(planet1NameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Earth", ((Scalar<string>)planet1NameProp.Value).Value);
        
        var planet1SizeProp = planet1.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(planet1SizeProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(10, ((Scalar<int>)planet1SizeProp.Value).Value);
        
        // Check planet 2
        var planet2Prop = planets.Properties.First(p => p.Key == "2");
        Assert.IsInstanceOfType(planet2Prop.Value, typeof(SaveObject));
        var planet2 = (SaveObject)planet2Prop.Value;
        
        var planet2NameProp = planet2.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(planet2NameProp.Value, typeof(Scalar<string>));
        Assert.AreEqual("Mars", ((Scalar<string>)planet2NameProp.Value).Value);
        
        var planet2SizeProp = planet2.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(planet2SizeProp.Value, typeof(Scalar<int>));
        Assert.AreEqual(8, ((Scalar<int>)planet2SizeProp.Value).Value);
    }

    [TestMethod]
    public void Parse_RepeatingKey_Objects_ReturnsAllValues()
    {
        var input = """
        object_with_repeating_keys={
            section={
                design="STARHOLD_STARBASE_SECTION"
                slot="core"
                weapon={
                    index=47
                    template="MEDIUM_MASS_DRIVER_1"
                    component_slot="MEDIUM_GUN_01"
                }
                weapon={
                    index=48
                    template="MEDIUM_MASS_DRIVER_1"
                    component_slot="MEDIUM_GUN_02"
                }
            }
            section={
                design="ASSEMBLYYARD_STARBASE_SECTION"
                slot="1"
            }
            section={
                design="REFINERY_STARBASE_SECTION"
                slot="2"
            }
        }
        """;
        
        // Act
        var root = Parser.Parse(input);

        // Assert        
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have 1 properties");
        var repeatingKeyObject = root.Properties[0];
        Assert.AreEqual("object_with_repeating_keys", repeatingKeyObject.Key, "Property key should be 'object_with_repeating_keys'");
        
        Assert.IsInstanceOfType(repeatingKeyObject.Value, typeof(SaveObject));
        var repeatingKeyObjectValue = (SaveObject)repeatingKeyObject.Value;
        
        Assert.AreEqual(3, repeatingKeyObjectValue.Properties.Count, "Repeating key object should have 3 properties");
    }

    [TestMethod]
    public void Parse_UnnamedObjectArray_ReturnsCorrectStructure()
    {
        // Arrange
        string input = @"
        terms={
            discrete_terms={
                { key=specialist_type value=specialist_none }
                { key=subject_integration value=subject_can_not_be_integrated }
            }
            resource_terms={
                { key=resource_subsidies_basic value=0.0 }
                { key=resource_subsidies_advanced value=25.5 }
            }
        }";

        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        
        Assert.AreEqual(1, root.Properties.Count);
        var termsProp = root.Properties[0];
        Assert.AreEqual("terms", termsProp.Key);
        
        Assert.IsInstanceOfType(termsProp.Value, typeof(SaveObject));
        var terms = (SaveObject)termsProp.Value;
        
        // Check discrete terms
        var discreteTermsProp = terms.Properties.First(p => p.Key == "discrete_terms");
        Assert.IsInstanceOfType(discreteTermsProp.Value, typeof(SaveArray));
        var discreteTerms = (SaveArray)discreteTermsProp.Value;
        
        Assert.AreEqual(2, discreteTerms.Items.Count);
        
        // Check first discrete term
        var firstTermObj = (SaveObject)discreteTerms.Items[0];
        var firstTermKey = firstTermObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(firstTermKey.Value, typeof(Scalar<string>));
        Assert.AreEqual("specialist_type", ((Scalar<string>)firstTermKey.Value).Value);
        
        var firstTermValue = firstTermObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(firstTermValue.Value, typeof(Scalar<string>));
        Assert.AreEqual("specialist_none", ((Scalar<string>)firstTermValue.Value).Value);
        
        // Check second discrete term
        var secondTermObj = (SaveObject)discreteTerms.Items[1];
        var secondTermKey = secondTermObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(secondTermKey.Value, typeof(Scalar<string>));
        Assert.AreEqual("subject_integration", ((Scalar<string>)secondTermKey.Value).Value);
        
        var secondTermValue = secondTermObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(secondTermValue.Value, typeof(Scalar<string>));
        Assert.AreEqual("subject_can_not_be_integrated", ((Scalar<string>)secondTermValue.Value).Value);
        
        // Check resource terms
        var resourceTermsProp = terms.Properties.First(p => p.Key == "resource_terms");
        Assert.IsInstanceOfType(resourceTermsProp.Value, typeof(SaveArray));
        var resourceTerms = (SaveArray)resourceTermsProp.Value;
        
        Assert.AreEqual(2, resourceTerms.Items.Count);
        
        // Check first resource term
        var firstResourceObj = (SaveObject)resourceTerms.Items[0];
        var firstResourceKey = firstResourceObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(firstResourceKey.Value, typeof(Scalar<string>));
        Assert.AreEqual("resource_subsidies_basic", ((Scalar<string>)firstResourceKey.Value).Value);
        
        var firstResourceValue = firstResourceObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(firstResourceValue.Value, typeof(Scalar<float>));
        Assert.AreEqual(0.0f, ((Scalar<float>)firstResourceValue.Value).Value);
        
        // Check second resource term
        var secondResourceObj = (SaveObject)resourceTerms.Items[1];
        var secondResourceKey = secondResourceObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(secondResourceKey.Value, typeof(Scalar<string>));
        Assert.AreEqual("resource_subsidies_advanced", ((Scalar<string>)secondResourceKey.Value).Value);
        
        var secondResourceValue = secondResourceObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(secondResourceValue.Value, typeof(Scalar<float>));
        Assert.AreEqual(25.5f, ((Scalar<float>)secondResourceValue.Value).Value);
    }
    
    [TestMethod]
    public void Parse_EmptyObjectArray_ReturnsEmptyArray()
    {
        // Arrange
        string input = """
                       culling_value=
                       {
                       	{
                       	}
                       	{
                       	}
                       	{
                       	}
                       }
                       """;
        
        // Act
        var root = Parser.Parse(input);

        // Assert
        Assert.IsInstanceOfType(root, typeof(SaveObject));
        
        Assert.AreEqual(1, root.Properties.Count);
        var cullingValueProp = root.Properties[0];
        
        Assert.AreEqual("culling_value", cullingValueProp.Key);
        Assert.IsInstanceOfType(cullingValueProp.Value, typeof(SaveArray));
        var cullingValueArray = (SaveArray)cullingValueProp.Value;
        Assert.AreEqual(3, cullingValueArray.Items.Count);
        
        // Check each item in the array
        foreach (var item in cullingValueArray.Items)
        {
            Assert.IsInstanceOfType(item, typeof(SaveObject));
            var itemObj = (SaveObject)item;
            Assert.AreEqual(0, itemObj.Properties.Count, "Each item should be an empty object");
        }
    }
}