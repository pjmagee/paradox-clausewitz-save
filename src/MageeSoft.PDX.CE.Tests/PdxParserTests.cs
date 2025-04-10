using MageeSoft.PDX.CE2;

namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class PdxParserTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Parse_List_StringKeysPdxScalarValues()
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
        var root = PdxSaveReader.Read(input.AsMemory());
        
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        
        var shipNamesProp = root.Properties[0];
        Assert.AreEqual("ship_names", shipNamesProp.Key, "Property key should be 'ship_names'");
        Assert.IsInstanceOfType(shipNamesProp.Value, typeof(PdxObject));
        
        var shipNames = (PdxObject)shipNamesProp.Value;
        Assert.AreEqual(3, shipNames.Properties.Count, "Ship names should have 3 properties");
        Assert.IsInstanceOfType(shipNames.Properties[0].Value, typeof(PdxScalar<int>));
        
        var firstShipName = shipNames.Properties[0];
        Assert.AreEqual("REP3_SHIP_Erid-Sur", firstShipName.Key, "First ship name key should be 'REP3_SHIP_Erid-Sur'");
        Assert.AreEqual(1, ((PdxScalar<int>)firstShipName.Value).Value, "First ship name value should be 1");
        Assert.IsInstanceOfType(shipNames.Properties[1].Value, typeof(PdxScalar<int>));
        
        var secondShipName = shipNames.Properties[1];
        Assert.AreEqual("%SEQ%", secondShipName.Key, "Second ship name key should be '%SEQ%'");
        Assert.AreEqual(14, ((PdxScalar<int>)secondShipName.Value).Value, "Second ship name value should be 14");
        Assert.IsInstanceOfType(shipNames.Properties[2].Value, typeof(PdxScalar<int>));
        
        var thirdShipName = shipNames.Properties[2];
        Assert.AreEqual("REP3_SHIP_Lorod-Gexad", thirdShipName.Key, "Third ship name key should be 'REP3_SHIP_Lorod-Gexad'");
        Assert.AreEqual(1, ((PdxScalar<int>)thirdShipName.Value).Value, "Third ship name value should be 1");
        Assert.IsInstanceOfType(shipNames.Properties[2].Value, typeof(PdxScalar<int>));
    }
    
    [TestMethod]
    public void Parse_List_Integers()
    {
        // Arrange
        int[] expectedValues = { 22, 27, 30, 37, 40 };
        string input = "test={" + string.Join(" ", expectedValues) + "}";
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var testProp = root.Properties[0];
        Assert.AreEqual("test", testProp.Key, "Property key should be 'test'");
        
        Assert.IsInstanceOfType(testProp.Value, typeof(PdxArray));
        var array = (PdxArray)testProp.Value;
        
        Assert.AreEqual(expectedValues.Length, array.Items.Count, "Array should have same number of items");
       
        for (int i = 0; i < expectedValues.Length; i++)
        {
            Assert.IsInstanceOfType(array.Items[i], typeof(PdxScalar<int>));
            var PdxScalar = (PdxScalar<int>)array.Items[i];
            Assert.AreEqual(expectedValues[i], PdxScalar.Value, $"Item {i} should have value {i + 1}");
        }
    }

    [TestMethod]
    public void Parse_Objects_And_Nested_Objects()
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
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
                
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var countryProp = root.Properties[0];
        Assert.AreEqual("country", countryProp.Key, "Property key should be 'country'");
        
        Assert.IsInstanceOfType(countryProp.Value, typeof(PdxObject));
        var country = (PdxObject)countryProp.Value;
        
        // Check country properties
        Assert.AreEqual(3, country.Properties.Count, "Country should have 3 properties");
        
        // Check name
        var nameProp = country.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(nameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Test Empire", ((PdxScalar<string>)nameProp.Value).Value);
        
        // Check capital
        var capitalProp = country.Properties.First(p => p.Key == "capital");
        Assert.IsInstanceOfType(capitalProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(5, ((PdxScalar<int>)capitalProp.Value).Value);
        
        // Check resources
        var resourcesProp = country.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(PdxObject));
        var resources = (PdxObject)resourcesProp.Value;
        
        // Check energy
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(100, ((PdxScalar<int>)energyProp.Value).Value);
        
        // Check minerals
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(200, ((PdxScalar<int>)mineralsProp.Value).Value);
    }

    [TestMethod]
    public void Parse_YesNo_Booleans()
    {
        // Arrange
        string input = @"
        settings={
            ironman=yes
            multiplayer=no
        }";

        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var settingsProp = root.Properties[0];
        Assert.AreEqual("settings", settingsProp.Key, "Property key should be 'settings'");
        
        Assert.IsInstanceOfType(settingsProp.Value, typeof(PdxObject));
        var settings = (PdxObject)settingsProp.Value;
        
        // Check ironman (yes)
        var ironmanProp = settings.Properties.First(p => p.Key == "ironman");
        Assert.IsInstanceOfType(ironmanProp.Value, typeof(PdxScalar<bool>));
        Assert.IsTrue(((PdxScalar<bool>)ironmanProp.Value).Value);
        
        // Check multiplayer (no)
        var multiplayerProp = settings.Properties.First(p => p.Key == "multiplayer");
        Assert.IsInstanceOfType(multiplayerProp.Value, typeof(PdxScalar<bool>));
        Assert.IsFalse(((PdxScalar<bool>)multiplayerProp.Value).Value);
    }

    [TestMethod]
    public void Parse_Dates()
    {
        // Arrange
        string input = @"
        game={
            start_date=""2200.01.01""
            current_date=""2250.05.12""
        }";

        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var gameProp = root.Properties[0];
        Assert.AreEqual("game", gameProp.Key, "Property key should be 'game'");
        
        Assert.IsInstanceOfType(gameProp.Value, typeof(PdxObject));
        var game = (PdxObject)gameProp.Value;
        
        // Check start_date
        var startDateProp = game.Properties.First(p => p.Key == "start_date");
        Assert.IsInstanceOfType(startDateProp.Value, typeof(PdxScalar<DateTime>));
        Assert.AreEqual(new DateTime(2200, 1, 1), ((PdxScalar<DateTime>)startDateProp.Value).Value);
        
        // Check current_date
        var currentDateProp = game.Properties.First(p => p.Key == "current_date");
        Assert.IsInstanceOfType(currentDateProp.Value, typeof(PdxScalar<DateTime>));
        Assert.AreEqual(new DateTime(2250, 5, 12), ((PdxScalar<DateTime>)currentDateProp.Value).Value);
    }

    [TestMethod]
    public void Parse_List_Ints_And_Floats()
    {
        // Arrange
        string input = @"
        mixed_array={
            42
            9223372036854775807
            3.14
        }";
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var arrayProp = root.Properties[0];
        Assert.AreEqual("mixed_array", arrayProp.Key, "Property key should be 'mixed_array'");
        
        Assert.IsInstanceOfType(arrayProp.Value, typeof(PdxArray));
        var mixedArray = (PdxArray)arrayProp.Value;
        
        Assert.AreEqual(3, mixedArray.Items.Count, "Array should have exactly three values");
        
        // Check integer value
        Assert.IsInstanceOfType(mixedArray.Items[0], typeof(PdxScalar<int>));
        Assert.AreEqual(42, ((PdxScalar<int>)mixedArray.Items[0]).Value);
        
        // Check long value
        Assert.IsInstanceOfType(mixedArray.Items[1], typeof(PdxScalar<long>));
        Assert.AreEqual(9223372036854775807, ((PdxScalar<long>)mixedArray.Items[1]).Value);
        
        // Check float value
        Assert.IsInstanceOfType(mixedArray.Items[2], typeof(PdxScalar<float>));
        Assert.AreEqual(3.14f, ((PdxScalar<float>)mixedArray.Items[2]).Value, 0.0001f);
    }

    [TestMethod]
    public void Parse_Different_Numeric_PdxScalars()
    {
        // Arrange
        string input = @"
        values={
            integer=42
            float=3.14
            large_integer=9223372036854775807
        }";
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var valuesProp = root.Properties[0];
        Assert.AreEqual("values", valuesProp.Key, "Property key should be 'values'");
        
        Assert.IsInstanceOfType(valuesProp.Value, typeof(PdxObject));
        var values = (PdxObject)valuesProp.Value;
        
        // Check integer
        var integerProp = values.Properties.First(p => p.Key == "integer");
        Assert.IsInstanceOfType(integerProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(42, ((PdxScalar<int>)integerProp.Value).Value);
        
        // Check float
        var floatProp = values.Properties.First(p => p.Key == "float");
        Assert.IsInstanceOfType(floatProp.Value, typeof(PdxScalar<float>));
        Assert.AreEqual(3.14f, ((PdxScalar<float>)floatProp.Value).Value, 0.0001f);
        
        // Check large integer
        var largeIntegerProp = values.Properties.First(p => p.Key == "large_integer");
        Assert.IsInstanceOfType(largeIntegerProp.Value, typeof(PdxScalar<long>));
        Assert.AreEqual(9223372036854775807, ((PdxScalar<long>)largeIntegerProp.Value).Value);
    }

    [TestMethod]
    public void Parse_Empty_Object()
    {
        // Arrange
        string input = "empty={}";
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var emptyProp = root.Properties[0];
        Assert.AreEqual("empty", emptyProp.Key, "Property key should be 'empty'");
        
        Assert.IsInstanceOfType(emptyProp.Value, typeof(PdxObject));
        var empty = (PdxObject)emptyProp.Value;
        
        Assert.AreEqual(0, empty.Properties.Count, "Empty object should have 0 properties");
    }
    
    [TestMethod]
    public void Parse_PdxScalar_Guid()
    {
        // Arrange
        string input = @"
        guids={
            id=""00000000-0000-0000-0000-000000000000""
            random_id=""12345678-1234-5678-1234-567812345678""
        }";
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var guidsProp = root.Properties[0];
        Assert.AreEqual("guids", guidsProp.Key, "Property key should be 'guids'");
        
        Assert.IsInstanceOfType(guidsProp.Value, typeof(PdxObject));
        var guids = (PdxObject)guidsProp.Value;
        
        // Check empty GUID
        var idProp = guids.Properties.First(p => p.Key == "id");
        Assert.IsInstanceOfType(idProp.Value, typeof(PdxScalar<Guid>));
        Assert.AreEqual(Guid.Empty, ((PdxScalar<Guid>)idProp.Value).Value);
        
        // Check random GUID
        var randomIdProp = guids.Properties.First(p => p.Key == "random_id");
        Assert.IsInstanceOfType(randomIdProp.Value, typeof(PdxScalar<Guid>));
        Assert.AreEqual(new Guid("12345678-1234-5678-1234-567812345678"), ((PdxScalar<Guid>)randomIdProp.Value).Value);
    }
    
    [TestMethod]
    public void Parse_Object_Array()
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
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var nestedArraysProp = root.Properties[0];
        Assert.AreEqual("nested_arrays", nestedArraysProp.Key, "Property key should be 'nested_arrays'");
        
        Assert.IsInstanceOfType(nestedArraysProp.Value, typeof(PdxObject));
        var nestedArrays = (PdxObject)nestedArraysProp.Value;
        
        // Check simple array
        var simpleArrayProp = nestedArrays.Properties.First(p => p.Key == "simple_array");
        Assert.IsInstanceOfType(simpleArrayProp.Value, typeof(PdxArray));
        var simpleArray = (PdxArray)simpleArrayProp.Value;
        
        Assert.AreEqual(3, simpleArray.Items.Count, "Simple array should have 3 items");
        for (int i = 0; i < 3; i++)
        {
            Assert.IsInstanceOfType(simpleArray.Items[i], typeof(PdxScalar<int>));
            Assert.AreEqual(i + 1, ((PdxScalar<int>)simpleArray.Items[i]).Value);
        }
        
        // Check complex array
        var complexArrayProp = nestedArrays.Properties.First(p => p.Key == "complex_array");
        Assert.IsInstanceOfType(complexArrayProp.Value, typeof(PdxArray));
        var complexArray = (PdxArray)complexArrayProp.Value;
        
        Assert.AreEqual(2, complexArray.Items.Count, "Complex array should have 2 items");
        
        // Check first item
        Assert.IsInstanceOfType(complexArray.Items[0], typeof(PdxObject));
        var item1 = (PdxObject)complexArray.Items[0];
        
        var item1NameProp = item1.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(item1NameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Item 1", ((PdxScalar<string>)item1NameProp.Value).Value);
        
        var item1ValueProp = item1.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(item1ValueProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(10, ((PdxScalar<int>)item1ValueProp.Value).Value);
        
        // Check second item
        Assert.IsInstanceOfType(complexArray.Items[1], typeof(PdxObject));
        var item2 = (PdxObject)complexArray.Items[1];
        
        var item2NameProp = item2.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(item2NameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Item 2", ((PdxScalar<string>)item2NameProp.Value).Value);
        
        var item2ValueProp = item2.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(item2ValueProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(20, ((PdxScalar<int>)item2ValueProp.Value).Value);
    }
    
    [TestMethod]
    public void Parse_Deep_Nested_Objects()
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
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        
        // Check galaxy
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var galaxyProp = root.Properties[0];
        Assert.AreEqual("galaxy", galaxyProp.Key, "Property key should be 'galaxy'");
        Assert.IsInstanceOfType(galaxyProp.Value, typeof(PdxObject));
        var galaxy = (PdxObject)galaxyProp.Value;
        
        // Check planets
        var planetsProp = galaxy.Properties.First(p => p.Key == "planets");
        Assert.IsInstanceOfType(planetsProp.Value, typeof(PdxObject));
        var planets = (PdxObject)planetsProp.Value;
        
        // Check planet 1
        var planet1Prop = planets.Properties.First(p => p.Key == "1");
        Assert.IsInstanceOfType(planet1Prop.Value, typeof(PdxObject));
        var planet1 = (PdxObject)planet1Prop.Value;
        
        // Check planet name
        var planetNameProp = planet1.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(planetNameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Earth", ((PdxScalar<string>)planetNameProp.Value).Value);
        
        // Check planet size
        var planetSizeProp = planet1.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(planetSizeProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(10, ((PdxScalar<int>)planetSizeProp.Value).Value);
        
        // Check moons
        var moonsProp = planet1.Properties.First(p => p.Key == "moons");
        Assert.IsInstanceOfType(moonsProp.Value, typeof(PdxObject));
        var moons = (PdxObject)moonsProp.Value;
        
        // Check moon 1
        var moon1Prop = moons.Properties.First(p => p.Key == "1");
        Assert.IsInstanceOfType(moon1Prop.Value, typeof(PdxObject));
        var moon1 = (PdxObject)moon1Prop.Value;
        
        // Check moon name
        var moonNameProp = moon1.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(moonNameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Moon", ((PdxScalar<string>)moonNameProp.Value).Value);
        
        // Check moon size
        var moonSizeProp = moon1.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(moonSizeProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(2, ((PdxScalar<int>)moonSizeProp.Value).Value);
        
        // Check resources
        var resourcesProp = planet1.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(PdxObject));
        var resources = (PdxObject)resourcesProp.Value;
        
        // Check energy
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(100, ((PdxScalar<int>)energyProp.Value).Value);
        
        // Check minerals
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(PdxObject));
        var minerals = (PdxObject)mineralsProp.Value;
        
        // Check minerals base
        var mineralsBaseProp = minerals.Properties.First(p => p.Key == "base");
        Assert.IsInstanceOfType(mineralsBaseProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(50, ((PdxScalar<int>)mineralsBaseProp.Value).Value);
        
        // Check minerals bonus
        var mineralsBonusProp = minerals.Properties.First(p => p.Key == "bonus");
        Assert.IsInstanceOfType(mineralsBonusProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(25, ((PdxScalar<int>)mineralsBonusProp.Value).Value);
    }
    
    [TestMethod]
    public void Parse_Many_TopLevel_Properties()
    {
        // Arrange
        string input = @"
        name=""Test Empire""
        capital=5
        resources={ energy=100 minerals=200 }
        flags={ is_xenophile=yes is_pacifist=no }
        ";
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
                
        Assert.AreEqual(4, root.Properties.Count, "Root should have 4 properties");
        
        // Check name
        var nameProp = root.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(nameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Test Empire", ((PdxScalar<string>)nameProp.Value).Value);
        
        // Check capital
        var capitalProp = root.Properties.First(p => p.Key == "capital");
        Assert.IsInstanceOfType(capitalProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(5, ((PdxScalar<int>)capitalProp.Value).Value);
        
        // Check resources
        var resourcesProp = root.Properties.First(p => p.Key == "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(PdxObject));
        var resources = (PdxObject)resourcesProp.Value;
        
        var energyProp = resources.Properties.First(p => p.Key == "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(100, ((PdxScalar<int>)energyProp.Value).Value);
        
        var mineralsProp = resources.Properties.First(p => p.Key == "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(200, ((PdxScalar<int>)mineralsProp.Value).Value);
        
        // Check flags
        var flagsProp = root.Properties.First(p => p.Key == "flags");
        Assert.IsInstanceOfType(flagsProp.Value, typeof(PdxObject));
        var flags = (PdxObject)flagsProp.Value;
        
        var xenophileProp = flags.Properties.First(p => p.Key == "is_xenophile");
        Assert.IsInstanceOfType(xenophileProp.Value, typeof(PdxScalar<bool>));
        Assert.IsTrue(((PdxScalar<bool>)xenophileProp.Value).Value);
        
        var pacifistProp = flags.Properties.First(p => p.Key == "is_pacifist");
        Assert.IsInstanceOfType(pacifistProp.Value, typeof(PdxScalar<bool>));
        Assert.IsFalse(((PdxScalar<bool>)pacifistProp.Value).Value);
    }

    [TestMethod]
    public void Parse_Array_Of_Indexed_Items()
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
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var planetsProp = root.Properties[0];
        Assert.AreEqual("planets", planetsProp.Key, "Property key should be 'planets'");
        
        Assert.IsInstanceOfType(planetsProp.Value, typeof(PdxObject));
        var planets = (PdxObject)planetsProp.Value;
        
        // Should have 2 planets with numeric keys
        Assert.AreEqual(2, planets.Properties.Count, "Planets should have 2 properties");
        
        // Check planet 1
        var planet1Prop = planets.Properties.First(p => p.Key == "1");
        Assert.IsInstanceOfType(planet1Prop.Value, typeof(PdxObject));
        var planet1 = (PdxObject)planet1Prop.Value;
        
        var planet1NameProp = planet1.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(planet1NameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Earth", ((PdxScalar<string>)planet1NameProp.Value).Value);
        
        var planet1SizeProp = planet1.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(planet1SizeProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(10, ((PdxScalar<int>)planet1SizeProp.Value).Value);
        
        // Check planet 2
        var planet2Prop = planets.Properties.First(p => p.Key == "2");
        Assert.IsInstanceOfType(planet2Prop.Value, typeof(PdxObject));
        var planet2 = (PdxObject)planet2Prop.Value;
        
        var planet2NameProp = planet2.Properties.First(p => p.Key == "name");
        Assert.IsInstanceOfType(planet2NameProp.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("Mars", ((PdxScalar<string>)planet2NameProp.Value).Value);
        
        var planet2SizeProp = planet2.Properties.First(p => p.Key == "size");
        Assert.IsInstanceOfType(planet2SizeProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(8, ((PdxScalar<int>)planet2SizeProp.Value).Value);
    }

    [TestMethod]
    public void Parse_Repeating_Key_Objects()
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
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert        
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        
        Assert.AreEqual(1, root.Properties.Count, "Root should have 1 properties");
        var repeatingKeyObject = root.Properties[0];
        Assert.AreEqual("object_with_repeating_keys", repeatingKeyObject.Key, "Property key should be 'object_with_repeating_keys'");
        
        Assert.IsInstanceOfType(repeatingKeyObject.Value, typeof(PdxObject));
        var repeatingKeyObjectValue = (PdxObject)repeatingKeyObject.Value;
        
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
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        Assert.AreEqual(1, root.Properties.Count);
        var termsProp = root.Properties[0];
        Assert.AreEqual("terms", termsProp.Key);
        
        Assert.IsInstanceOfType(termsProp.Value, typeof(PdxObject));
        var terms = (PdxObject)termsProp.Value;
        
        // Check discrete terms
        var discreteTermsProp = terms.Properties.First(p => p.Key == "discrete_terms");
        Assert.IsInstanceOfType(discreteTermsProp.Value, typeof(PdxArray));
        var discreteTerms = (PdxArray)discreteTermsProp.Value;
        
        Assert.AreEqual(2, discreteTerms.Items.Count);
        
        // Check first discrete term
        var firstTermObj = (PdxObject)discreteTerms.Items[0];
        var firstTermKey = firstTermObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(firstTermKey.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("specialist_type", ((PdxScalar<string>)firstTermKey.Value).Value);
        
        var firstTermValue = firstTermObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(firstTermValue.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("specialist_none", ((PdxScalar<string>)firstTermValue.Value).Value);
        
        // Check second discrete term
        var secondTermObj = (PdxObject)discreteTerms.Items[1];
        var secondTermKey = secondTermObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(secondTermKey.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("subject_integration", ((PdxScalar<string>)secondTermKey.Value).Value);
        
        var secondTermValue = secondTermObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(secondTermValue.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("subject_can_not_be_integrated", ((PdxScalar<string>)secondTermValue.Value).Value);
        
        // Check resource terms
        var resourceTermsProp = terms.Properties.First(p => p.Key == "resource_terms");
        Assert.IsInstanceOfType(resourceTermsProp.Value, typeof(PdxArray));
        var resourceTerms = (PdxArray)resourceTermsProp.Value;
        
        Assert.AreEqual(2, resourceTerms.Items.Count);
        
        // Check first resource term
        var firstResourceObj = (PdxObject)resourceTerms.Items[0];
        var firstResourceKey = firstResourceObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(firstResourceKey.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("resource_subsidies_basic", ((PdxScalar<string>)firstResourceKey.Value).Value);
        
        var firstResourceValue = firstResourceObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(firstResourceValue.Value, typeof(PdxScalar<float>));
        Assert.AreEqual(0.0f, ((PdxScalar<float>)firstResourceValue.Value).Value);
        
        // Check second resource term
        var secondResourceObj = (PdxObject)resourceTerms.Items[1];
        var secondResourceKey = secondResourceObj.Properties.First(p => p.Key == "key");
        Assert.IsInstanceOfType(secondResourceKey.Value, typeof(PdxScalar<string>));
        Assert.AreEqual("resource_subsidies_advanced", ((PdxScalar<string>)secondResourceKey.Value).Value);
        
        var secondResourceValue = secondResourceObj.Properties.First(p => p.Key == "value");
        Assert.IsInstanceOfType(secondResourceValue.Value, typeof(PdxScalar<float>));
        Assert.AreEqual(25.5f, ((PdxScalar<float>)secondResourceValue.Value).Value);
    }
    
    [TestMethod]
    public void Parse_Empty_Object_Array()
    {
        // Arrange
        string input = """
                       culling_value=
                       {
                       	{ }
                       	{ }
                        { }
                       }
                       """;
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        Assert.AreEqual(1, root.Properties.Count);
        var cullingValueProp = root.Properties[0];
        
        Assert.AreEqual("culling_value", cullingValueProp.Key);
        Assert.IsInstanceOfType(cullingValueProp.Value, typeof(PdxArray));
        var cullingValueArray = (PdxArray)cullingValueProp.Value;
        Assert.AreEqual(3, cullingValueArray.Items.Count);
        
        // Check each item in the array
        foreach (var item in cullingValueArray.Items)
        {
            Assert.IsInstanceOfType(item, typeof(PdxObject));
            var itemObj = (PdxObject)item;
            Assert.AreEqual(0, itemObj.Properties.Count, "Each item should be an empty object");
        }
    }


    [TestMethod]
    public void Parse_Object_List_With_Ids()
    {
        // Arrange
        //  67108916  is the id of the first intel
        //  218103860  is the id of the second intel

        string input = """
        {
            intel_manager=
            {
                intel=
                {
                    {
                        67108916 
                        {
                            intel=10
                            stale_intel=
                            {
                            }
                        }
                    }
    
                    {
                        218103860 
                        {
                            intel=10
                            stale_intel=
                            {
                            }
                        }
                    }
                }
            }
        }
        """;

        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        
        Assert.AreEqual(1, root.Properties.Count);
        var intelManagerProp = root.Properties[0];
        Assert.AreEqual("intel_manager", intelManagerProp.Key);
        
        Assert.IsInstanceOfType(intelManagerProp.Value, typeof(PdxObject));
        var intelManager = (PdxObject)intelManagerProp.Value;
        
        Assert.AreEqual(1, intelManager.Properties.Count);
        var intelProp = intelManager.Properties[0];
        Assert.AreEqual("intel", intelProp.Key);
        
        Assert.IsInstanceOfType(intelProp.Value, typeof(PdxArray));
        var intelArray = (PdxArray)intelProp.Value;
        
        Assert.AreEqual(2, intelArray.Items.Count);

        // Each item in the 'intel' array is itself an array [ID, DataObject]
        
        // Check first item
        Assert.IsInstanceOfType(intelArray.Items[0], typeof(PdxArray)); 
        var firstItemArray = (PdxArray)intelArray.Items[0];
        Assert.AreEqual(2, firstItemArray.Items.Count);

        // the first item in the array is an ID
        Assert.IsInstanceOfType(firstItemArray.Items[0], typeof(PdxScalar<int>));
        Assert.AreEqual(67108916, ((PdxScalar<int>)firstItemArray.Items[0]).Value);
        
        // the second item in the array is a data object
        Assert.IsInstanceOfType(firstItemArray.Items[1], typeof(PdxObject));
        
        var firstDataObject = (PdxObject)firstItemArray.Items[1];
        Assert.AreEqual(2, firstDataObject.Properties.Count); // intel and stale_intel
        
        var firstIntelValueProp = firstDataObject.Properties.First(p => p.Key == "intel");
        Assert.IsInstanceOfType(firstIntelValueProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(10, ((PdxScalar<int>)firstIntelValueProp.Value).Value);
        
        var firstStaleIntelProp = firstDataObject.Properties.First(p => p.Key == "stale_intel");
        Assert.IsInstanceOfType(firstStaleIntelProp.Value, typeof(PdxObject));
        Assert.AreEqual(0, ((PdxObject)firstStaleIntelProp.Value).Properties.Count);

        
        // Check second item
        Assert.IsInstanceOfType(intelArray.Items[1], typeof(PdxArray)); // Second item is also an array [ID, DataObject]
        var secondItemArray = (PdxArray)intelArray.Items[1];
        Assert.AreEqual(2, secondItemArray.Items.Count); // Should contain ID and DataObject

        Assert.IsInstanceOfType(secondItemArray.Items[0], typeof(PdxScalar<int>));
        Assert.AreEqual(218103860, ((PdxScalar<int>)secondItemArray.Items[0]).Value);

        Assert.IsInstanceOfType(secondItemArray.Items[1], typeof(PdxObject));
        var secondDataObject = (PdxObject)secondItemArray.Items[1];
        Assert.AreEqual(2, secondDataObject.Properties.Count); // intel and stale_intel
        var secondIntelValueProp = secondDataObject.Properties.First(p => p.Key == "intel");
        Assert.IsInstanceOfType(secondIntelValueProp.Value, typeof(PdxScalar<int>));
        Assert.AreEqual(10, ((PdxScalar<int>)secondIntelValueProp.Value).Value);
        var secondStaleIntelProp = secondDataObject.Properties.First(p => p.Key == "stale_intel");
        Assert.IsInstanceOfType(secondStaleIntelProp.Value, typeof(PdxObject));
        Assert.AreEqual(0, ((PdxObject)secondStaleIntelProp.Value).Properties.Count);
    }

    [TestMethod]
    public void Parse_Multiple_Repeated_Keys_With_Specialized_Collection()
    {
        var input = """
        test_record={
            asteroid_postfix={ "413" "3254" }
            asteroid_postfix={ "1287" "7291" }
            asteroid_postfix={ "Alpha" "Beta" }
            asteroid_postfix={ "Gamma" "Delta" }
            name="Test System"
            id=123
        }
        """;
        
        // Act
        var root = PdxSaveReader.Read(input.AsMemory());
        
        // Check the property values in the original PdxObject
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        Assert.AreEqual(1, root.Properties.Count);

        Assert.IsTrue(root.TryGet<PdxObject>("test_record", out var testRecord));
        Assert.AreEqual(6, testRecord!.Properties.Count);
        
        // Verify we have 4 asteroid_postfix properties, each with array values
        var asteroidProps = testRecord.Properties
            .Where(p => p.Key == "asteroid_postfix")
            .ToList();
        
        Assert.AreEqual(4, asteroidProps.Count, "Should have 4 asteroid_postfix properties");
        
        // Check the first asteroid_postfix property
        Assert.IsInstanceOfType(asteroidProps[0].Value, typeof(PdxArray));
        var firstArray = (PdxArray)asteroidProps[0].Value;
        Assert.AreEqual(2, firstArray.Items.Count);
        Assert.AreEqual("413", ((PdxScalar<string>)firstArray.Items[0]).Value);
        Assert.AreEqual("3254", ((PdxScalar<string>)firstArray.Items[1]).Value);
    }
}