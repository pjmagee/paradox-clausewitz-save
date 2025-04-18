namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class PdxParserTests
{
    public TestContext TestContext { get; set; } = null!;

    // Helper method to find a property by key, using custom logic for the tests
    private KeyValuePair<IPdxScalar, IPdxElement> FindProperty(IEnumerable<KeyValuePair<IPdxScalar, IPdxElement>> properties, string key)
    {
        foreach (var property in properties)
        {
            if (property.Key is PdxString pdxString && pdxString.Value == key)
            {
                return property;
            }
            else if (property.Key is PdxInt pdxInt && pdxInt.Value.ToString() == key)
            {
                return property;
            }
            else if (property.Key is PdxLong pdxLong && pdxLong.Value.ToString() == key)
            {
                return property;
            }
        }

        Assert.Fail($"Property with key '{key}' not found");
        return default; // This will never be reached due to the Assert.Fail above
    }

    private KeyValuePair<IPdxScalar, IPdxElement> FindNumericProperty(IEnumerable<KeyValuePair<IPdxScalar, IPdxElement>> properties, int key)
    {
        foreach (var property in properties)
        {
            if (property.Key is PdxInt intKey && intKey.Value == key)
            {
                return property;
            }
        }
        
        Assert.Fail($"Property with key '{key}' not found");
        return default; // This line will never be reached due to Assert.Fail
    }

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
        Assert.IsInstanceOfType(shipNamesProp.Key, typeof(PdxString));
        Assert.AreEqual("ship_names", ((PdxString)shipNamesProp.Key).Value, "Property key should be 'ship_names'");
        Assert.IsInstanceOfType(shipNamesProp.Value, typeof(PdxObject));

        var shipNames = (PdxObject)shipNamesProp.Value;
        Assert.AreEqual(3, shipNames.Properties.Count, "Ship names should have 3 properties");
        Assert.IsInstanceOfType(shipNames.Properties[0].Value, typeof(PdxInt));

        var firstShipName = shipNames.Properties[0];
        Assert.IsInstanceOfType(firstShipName.Key, typeof(PdxString));
        Assert.AreEqual("REP3_SHIP_Erid-Sur", ((PdxString)firstShipName.Key).Value, "First ship name key should be 'REP3_SHIP_Erid-Sur'");
        Assert.AreEqual(1, ((PdxInt)firstShipName.Value).Value, "First ship name value should be 1");
        Assert.IsInstanceOfType(shipNames.Properties[1].Value, typeof(PdxInt));

        var secondShipName = shipNames.Properties[1];
        Assert.IsInstanceOfType(secondShipName.Key, typeof(PdxString));
        Assert.AreEqual("%SEQ%", ((PdxString)secondShipName.Key).Value, "Second ship name key should be '%SEQ%'");
        Assert.AreEqual(14, ((PdxInt)secondShipName.Value).Value, "Second ship name value should be 14");
        Assert.IsInstanceOfType(shipNames.Properties[2].Value, typeof(PdxInt));

        var thirdShipName = shipNames.Properties[2];
        Assert.IsInstanceOfType(thirdShipName.Key, typeof(PdxString));
        Assert.AreEqual("REP3_SHIP_Lorod-Gexad", ((PdxString)thirdShipName.Key).Value, "Third ship name key should be 'REP3_SHIP_Lorod-Gexad'");
        Assert.AreEqual(1, ((PdxInt)thirdShipName.Value).Value, "Third ship name value should be 1");
        Assert.IsInstanceOfType(shipNames.Properties[2].Value, typeof(PdxInt));
    }

    [TestMethod]
    public void Parse_List_Integers()
    {
        // Arrange
        int[] expectedValues =
        {
            22,
            27,
            30,
            37,
            40
        };

        string input = "test={" + string.Join(" ", expectedValues) + "}";

        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var testProp = root.Properties[0];
        Assert.IsInstanceOfType(testProp.Key, typeof(PdxString));
        Assert.AreEqual("test", ((PdxString)testProp.Key).Value, "Property key should be 'test'");

        Assert.IsInstanceOfType(testProp.Value, typeof(PdxArray));
        var array = (PdxArray)testProp.Value;

        Assert.AreEqual(expectedValues.Length, array.Items.Count, "Array should have same number of items");

        for (int i = 0; i < expectedValues.Length; i++)
        {
            Assert.IsInstanceOfType(array.Items[i], typeof(PdxInt));
            var pdxInt = (PdxInt)array.Items[i];
            Assert.AreEqual(expectedValues[i], pdxInt.Value, $"Item {i} should have value {expectedValues[i]}");
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
        Assert.IsInstanceOfType(countryProp.Key, typeof(PdxString));
        Assert.AreEqual("country", ((PdxString)countryProp.Key).Value, "Property key should be 'country'");

        Assert.IsInstanceOfType(countryProp.Value, typeof(PdxObject));
        var country = (PdxObject)countryProp.Value;

        // Check country properties
        Assert.AreEqual(3, country.Properties.Count, "Country should have 3 properties");

        // Check name
        var nameProp = FindProperty(country.Properties, "name");
        Assert.IsInstanceOfType(nameProp.Value, typeof(PdxString));
        Assert.AreEqual("Test Empire", ((PdxString)nameProp.Value).Value);

        // Check capital
        var capitalProp = FindProperty(country.Properties, "capital");
        Assert.IsInstanceOfType(capitalProp.Value, typeof(PdxInt));
        Assert.AreEqual(5, ((PdxInt)capitalProp.Value).Value);

        // Check resources
        var resourcesProp = FindProperty(country.Properties, "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(PdxObject));
        var resources = (PdxObject)resourcesProp.Value;

        // Check energy
        var energyProp = FindProperty(resources.Properties, "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(PdxInt));
        Assert.AreEqual(100, ((PdxInt)energyProp.Value).Value);

        // Check minerals
        var mineralsProp = FindProperty(resources.Properties, "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(PdxInt));
        Assert.AreEqual(200, ((PdxInt)mineralsProp.Value).Value);
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
        Assert.IsInstanceOfType(settingsProp.Key, typeof(PdxString));
        Assert.AreEqual("settings", ((PdxString)settingsProp.Key).Value, "Property key should be 'settings'");

        Assert.IsInstanceOfType(settingsProp.Value, typeof(PdxObject));
        var settings = (PdxObject)settingsProp.Value;

        // Check ironman (yes)
        var ironmanProp = FindProperty(settings.Properties, "ironman");
        Assert.IsInstanceOfType(ironmanProp.Value, typeof(PdxBool));
        Assert.IsTrue(((PdxBool)ironmanProp.Value).Value);

        // Check multiplayer (no)
        var multiplayerProp = FindProperty(settings.Properties, "multiplayer");
        Assert.IsInstanceOfType(multiplayerProp.Value, typeof(PdxBool));
        Assert.IsFalse(((PdxBool)multiplayerProp.Value).Value);
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
        Assert.IsInstanceOfType(gameProp.Key, typeof(PdxString));
        Assert.AreEqual("game", ((PdxString)gameProp.Key).Value, "Property key should be 'game'");

        Assert.IsInstanceOfType(gameProp.Value, typeof(PdxObject));
        var game = (PdxObject)gameProp.Value;

        // Check start_date
        var startDateProp = FindProperty(game.Properties, "start_date");
        Assert.IsInstanceOfType(startDateProp.Value, typeof(PdxDate));
        Assert.AreEqual(new DateOnly(2200, 1, 1), ((PdxDate)startDateProp.Value).Value);

        // Check current_date
        var currentDateProp = FindProperty(game.Properties, "current_date");
        Assert.IsInstanceOfType(currentDateProp.Value, typeof(PdxDate));
        Assert.AreEqual(new DateOnly(2250, 5, 12), ((PdxDate)currentDateProp.Value).Value);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var arrayProp = root.Properties[0];
        Assert.IsInstanceOfType(arrayProp.Key, typeof(PdxString));
        Assert.AreEqual("mixed_array", ((PdxString)arrayProp.Key).Value, "Property key should be 'mixed_array'");

        Assert.IsInstanceOfType(arrayProp.Value, typeof(PdxArray));
        var mixedArray = (PdxArray)arrayProp.Value;

        Assert.AreEqual(3, mixedArray.Items.Count, "Array should have exactly three values");

        // Check integer value
        Assert.IsInstanceOfType(mixedArray.Items[0], typeof(PdxInt));
        Assert.AreEqual(42, ((PdxInt)mixedArray.Items[0]).Value);

        // Check long value
        Assert.IsInstanceOfType(mixedArray.Items[1], typeof(PdxLong));
        Assert.AreEqual(9223372036854775807, ((PdxLong)mixedArray.Items[1]).Value);

        // Check float value
        Assert.IsInstanceOfType(mixedArray.Items[2], typeof(PdxFloat));
        Assert.AreEqual(3.14f, ((PdxFloat)mixedArray.Items[2]).Value, 0.0001f);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var valuesProp = root.Properties[0];
        Assert.IsInstanceOfType(valuesProp.Key, typeof(PdxString));
        Assert.AreEqual("values", ((PdxString)valuesProp.Key).Value, "Property key should be 'values'");

        Assert.IsInstanceOfType(valuesProp.Value, typeof(PdxObject));
        var values = (PdxObject)valuesProp.Value;

        // Check integer
        var integerProp = FindProperty(values.Properties, "integer");
        Assert.IsInstanceOfType(integerProp.Value, typeof(PdxInt));
        Assert.AreEqual(42, ((PdxInt)integerProp.Value).Value);

        // Check float
        var floatProp = FindProperty(values.Properties, "float");
        Assert.IsInstanceOfType(floatProp.Value, typeof(PdxFloat));
        Assert.AreEqual(3.14f, ((PdxFloat)floatProp.Value).Value, 0.0001f);

        // Check large integer
        var largeIntegerProp = FindProperty(values.Properties, "large_integer");
        Assert.IsInstanceOfType(largeIntegerProp.Value, typeof(PdxLong));
        Assert.AreEqual(9223372036854775807, ((PdxLong)largeIntegerProp.Value).Value);
    }

    [TestMethod]
    public void Parse_Empty_Object()
    {
        // Arrange
        string input = "empty={}";

        // Act
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var emptyProp = root.Properties[0];
        Assert.IsInstanceOfType(emptyProp.Key, typeof(PdxString));
        Assert.AreEqual("empty", ((PdxString)emptyProp.Key).Value, "Property key should be 'empty'");

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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var guidsProp = root.Properties[0];
        Assert.IsInstanceOfType(guidsProp.Key, typeof(PdxString));
        Assert.AreEqual("guids", ((PdxString)guidsProp.Key).Value, "Property key should be 'guids'");

        Assert.IsInstanceOfType(guidsProp.Value, typeof(PdxObject));
        var guids = (PdxObject)guidsProp.Value;

        // Check empty GUID
        var idProp = FindProperty(guids.Properties, "id");
        Assert.IsInstanceOfType(idProp.Value, typeof(PdxGuid));
        Assert.AreEqual(Guid.Empty, ((PdxGuid)idProp.Value).Value);

        // Check random GUID
        var randomIdProp = FindProperty(guids.Properties, "random_id");
        Assert.IsInstanceOfType(randomIdProp.Value, typeof(PdxGuid));
        Assert.AreEqual(new Guid("12345678-1234-5678-1234-567812345678"), ((PdxGuid)randomIdProp.Value).Value);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var nestedArraysProp = root.Properties[0];
        Assert.AreEqual("nested_arrays", ((PdxString)nestedArraysProp.Key).Value, "Property key should be 'nested_arrays'");

        Assert.IsInstanceOfType(nestedArraysProp.Value, typeof(PdxObject));
        var nestedArrays = (PdxObject)nestedArraysProp.Value;

        // Check simple array
        var simpleArrayProp = FindProperty(nestedArrays.Properties, "simple_array");
        Assert.IsInstanceOfType(simpleArrayProp.Value, typeof(PdxArray));
        var simpleArray = (PdxArray)simpleArrayProp.Value;

        Assert.AreEqual(3, simpleArray.Items.Count, "Simple array should have 3 items");

        for (int i = 0; i < 3; i++)
        {
            Assert.IsInstanceOfType(simpleArray.Items[i], typeof(PdxInt));
            Assert.AreEqual(i + 1, ((PdxInt)simpleArray.Items[i]).Value);
        }

        // Check complex array
        var complexArrayProp = FindProperty(nestedArrays.Properties, "complex_array");
        Assert.IsInstanceOfType(complexArrayProp.Value, typeof(PdxArray));
        var complexArray = (PdxArray)complexArrayProp.Value;

        Assert.AreEqual(2, complexArray.Items.Count, "Complex array should have 2 items");

        // Check first item
        Assert.IsInstanceOfType(complexArray.Items[0], typeof(PdxObject));
        var item1 = (PdxObject)complexArray.Items[0];

        var item1NameProp = FindProperty(item1.Properties, "name");
        Assert.IsInstanceOfType(item1NameProp.Value, typeof(PdxString));
        Assert.AreEqual("Item 1", ((PdxString)item1NameProp.Value).Value);

        var item1ValueProp = FindProperty(item1.Properties, "value");
        Assert.IsInstanceOfType(item1ValueProp.Value, typeof(PdxInt));
        Assert.AreEqual(10, ((PdxInt)item1ValueProp.Value).Value);

        // Check second item
        Assert.IsInstanceOfType(complexArray.Items[1], typeof(PdxObject));
        var item2 = (PdxObject)complexArray.Items[1];

        var item2NameProp = FindProperty(item2.Properties, "name");
        Assert.IsInstanceOfType(item2NameProp.Value, typeof(PdxString));
        Assert.AreEqual("Item 2", ((PdxString)item2NameProp.Value).Value);

        var item2ValueProp = FindProperty(item2.Properties, "value");
        Assert.IsInstanceOfType(item2ValueProp.Value, typeof(PdxInt));
        Assert.AreEqual(20, ((PdxInt)item2ValueProp.Value).Value);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));


        // Check galaxy
        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var galaxyProp = root.Properties[0];
        
        Assert.AreEqual("galaxy", ((PdxString)galaxyProp.Key).Value, "Property key should be 'galaxy'");
        
        Assert.IsInstanceOfType(galaxyProp.Value, typeof(PdxObject));
        var galaxy = (PdxObject)galaxyProp.Value;

        // Check planets
        var planetsProp = FindProperty(galaxy.Properties, "planets");
        Assert.IsInstanceOfType(planetsProp.Value, typeof(PdxObject));
        var planets = (PdxObject)planetsProp.Value;

        // Check planet 1
        var planet1Prop = planets.Properties.First(p => p.Key is PdxInt intKey && intKey.Value == 1);
        Assert.IsInstanceOfType(planet1Prop.Value, typeof(PdxObject));
        var planet1 = (PdxObject)planet1Prop.Value;

        var planet1NameProp = planet1.Properties.First(p => p.Key is PdxString strKey && strKey.Value == "name");
        Assert.IsInstanceOfType(planet1NameProp.Value, typeof(PdxString));
        Assert.AreEqual("Earth", ((PdxString)planet1NameProp.Value).Value);

        var planet1SizeProp = planet1.Properties.First(p => p.Key is PdxString strKey && strKey.Value == "size");
        Assert.IsInstanceOfType(planet1SizeProp.Value, typeof(PdxInt));
        Assert.AreEqual(10, ((PdxInt)planet1SizeProp.Value).Value);

        // Check moons
        var moonsProp = FindProperty(planet1.Properties, "moons");
        Assert.IsInstanceOfType(moonsProp.Value, typeof(PdxObject));
        var moons = (PdxObject)moonsProp.Value;

        // Check moon 1
        var moon1Prop = FindProperty(moons.Properties, "1");
        Assert.IsInstanceOfType(moon1Prop.Value, typeof(PdxObject));
        var moon1 = (PdxObject)moon1Prop.Value;

        // Check moon name
        var moonNameProp = FindProperty(moon1.Properties, "name");
        Assert.IsInstanceOfType(moonNameProp.Value, typeof(PdxString));
        Assert.AreEqual("Moon", ((PdxString)moonNameProp.Value).Value);

        // Check moon size
        var moonSizeProp = FindProperty(moon1.Properties, "size");
        Assert.IsInstanceOfType(moonSizeProp.Value, typeof(PdxInt));
        Assert.AreEqual(2, ((PdxInt)moonSizeProp.Value).Value);

        // Check resources
        var resourcesProp = FindProperty(planet1.Properties, "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(PdxObject));
        var resources = (PdxObject)resourcesProp.Value;

        // Check energy
        var energyProp = FindProperty(resources.Properties, "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(PdxInt));
        Assert.AreEqual(100, ((PdxInt)energyProp.Value).Value);

        // Check minerals
        var mineralsProp = FindProperty(resources.Properties, "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(PdxObject));
        var minerals = (PdxObject)mineralsProp.Value;

        // Check minerals base
        var mineralsBaseProp = FindProperty(minerals.Properties, "base");
        Assert.IsInstanceOfType(mineralsBaseProp.Value, typeof(PdxInt));
        Assert.AreEqual(50, ((PdxInt)mineralsBaseProp.Value).Value);

        // Check minerals bonus
        var mineralsBonusProp = FindProperty(minerals.Properties, "bonus");
        Assert.IsInstanceOfType(mineralsBonusProp.Value, typeof(PdxInt));
        Assert.AreEqual(25, ((PdxInt)mineralsBonusProp.Value).Value);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(4, root.Properties.Count, "Root should have 4 properties");

        // Check name
        var nameProp = FindProperty(root.Properties, "name");
        Assert.IsInstanceOfType(nameProp.Value, typeof(PdxString));
        Assert.AreEqual("Test Empire", ((PdxString)nameProp.Value).Value);

        // Check capital
        var capitalProp = FindProperty(root.Properties, "capital");
        Assert.IsInstanceOfType(capitalProp.Value, typeof(PdxInt));
        Assert.AreEqual(5, ((PdxInt)capitalProp.Value).Value);

        // Check resources
        var resourcesProp = FindProperty(root.Properties, "resources");
        Assert.IsInstanceOfType(resourcesProp.Value, typeof(PdxObject));
        var resources = (PdxObject)resourcesProp.Value;

        var energyProp = FindProperty(resources.Properties, "energy");
        Assert.IsInstanceOfType(energyProp.Value, typeof(PdxInt));
        Assert.AreEqual(100, ((PdxInt)energyProp.Value).Value);

        var mineralsProp = FindProperty(resources.Properties, "minerals");
        Assert.IsInstanceOfType(mineralsProp.Value, typeof(PdxInt));
        Assert.AreEqual(200, ((PdxInt)mineralsProp.Value).Value);

        // Check flags
        var flagsProp = FindProperty(root.Properties, "flags");
        Assert.IsInstanceOfType(flagsProp.Value, typeof(PdxObject));
        var flags = (PdxObject)flagsProp.Value;

        var xenophileProp = FindProperty(flags.Properties, "is_xenophile");
        Assert.IsInstanceOfType(xenophileProp.Value, typeof(PdxBool));
        Assert.IsTrue(((PdxBool)xenophileProp.Value).Value);

        var pacifistProp = FindProperty(flags.Properties, "is_pacifist");
        Assert.IsInstanceOfType(pacifistProp.Value, typeof(PdxBool));
        Assert.IsFalse(((PdxBool)pacifistProp.Value).Value);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count, "Root should have exactly one property");
        var planetsProp = root.Properties[0];
        Assert.AreEqual("planets", ((PdxString)planetsProp.Key).Value);

        Assert.IsInstanceOfType(planetsProp.Value, typeof(PdxObject));
        var planets = (PdxObject)planetsProp.Value;

        // Should have 2 planets with numeric keys
        Assert.AreEqual(2, planets.Properties.Count, "Planets should have 2 properties");

        // Check planet 1
        var planet1Prop = planets.Properties.First(p => p.Key is PdxInt intKey && intKey.Value == 1);
        Assert.IsInstanceOfType(planet1Prop.Value, typeof(PdxObject));
        var planet1 = (PdxObject)planet1Prop.Value;

        var planet1NameProp = planet1.Properties.First(p => p.Key is PdxString strKey && strKey.Value == "name");
        Assert.IsInstanceOfType(planet1NameProp.Value, typeof(PdxString));
        Assert.AreEqual("Earth", ((PdxString)planet1NameProp.Value).Value);

        var planet1SizeProp = planet1.Properties.First(p => p.Key is PdxString strKey && strKey.Value == "size");
        Assert.IsInstanceOfType(planet1SizeProp.Value, typeof(PdxInt));
        Assert.AreEqual(10, ((PdxInt)planet1SizeProp.Value).Value);

        // Check planet 2
        var planet2Prop = planets.Properties.First(p => p.Key is PdxInt intKey && intKey.Value == 2);
        Assert.IsInstanceOfType(planet2Prop.Value, typeof(PdxObject));
        var planet2 = (PdxObject)planet2Prop.Value;

        var planet2NameProp = planet2.Properties.First(p => p.Key is PdxString strKey && strKey.Value == "name");
        Assert.IsInstanceOfType(planet2NameProp.Value, typeof(PdxString));
        Assert.AreEqual("Mars", ((PdxString)planet2NameProp.Value).Value);

        var planet2SizeProp = planet2.Properties.First(p => p.Key is PdxString strKey && strKey.Value == "size");
        Assert.IsInstanceOfType(planet2SizeProp.Value, typeof(PdxInt));
        Assert.AreEqual(8, ((PdxInt)planet2SizeProp.Value).Value);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert        
        Assert.IsInstanceOfType(root, typeof(PdxObject));


        Assert.AreEqual(1, root.Properties.Count, "Root should have 1 properties");
        var repeatingKeyObject = root.Properties[0];
        Assert.AreEqual("object_with_repeating_keys", ((PdxString)repeatingKeyObject.Key).Value);

        Assert.IsInstanceOfType(repeatingKeyObject.Value, typeof(PdxObject));
        var repeatingKeyObjectValue = (PdxObject)repeatingKeyObject.Value;

        Assert.AreEqual(3, repeatingKeyObjectValue.Properties.Count, "Repeating key object should have 3 properties");
    }

    [TestMethod]
    public void Parse_StringLiterals_TracksWasQuoted()
    {
        // Arrange
        string input = @"
        values={
            quoted=""Hello World""
            unquoted=Hello_World
        }";

        // Act
        var root = PdxSaveReader.Read(input.AsMemory());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        Assert.AreEqual(1, root.Properties.Count);

        var valuesProp = root.Properties[0];
        Assert.AreEqual("values", ((PdxString)valuesProp.Key).Value);
        Assert.IsInstanceOfType(valuesProp.Value, typeof(PdxObject));

        var values = (PdxObject)valuesProp.Value;
        Assert.AreEqual(2, values.Properties.Count);

        // Check quoted string
        var quotedProp = FindProperty(values.Properties, "quoted");
        Assert.IsInstanceOfType(quotedProp.Value, typeof(PdxString));
        var quotedStr = (PdxString)quotedProp.Value;
        Assert.AreEqual("Hello World", quotedStr.Value);
        Assert.IsTrue(quotedStr.WasQuoted, "String should track that it was quoted");

        // Check unquoted string
        var unquotedProp = FindProperty(values.Properties, "unquoted");
        Assert.IsInstanceOfType(unquotedProp.Value, typeof(PdxString));
        var unquotedStr = (PdxString)unquotedProp.Value;
        Assert.AreEqual("Hello_World", unquotedStr.Value);
        Assert.IsFalse(unquotedStr.WasQuoted, "String should track that it was not quoted");
    }

    [TestMethod]
    public void Parse_Complex_Object()
    {
        // Arrange
        string input = """
            test = {
                integer=42
                flag=yes
                nested={
                    name = "test"
                    value = 3.14
                }
                array={ 1 2 3 }
                date="1960.1.1"
            }
            """;

        // Act
        var result = PdxSaveReader.Read(input);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PdxObject));
        var obj = (PdxObject)result;
        Assert.AreEqual(1, obj.Properties.Count);

        var testProp = FindProperty(obj.Properties, "test");
        Assert.IsNotNull(testProp.Value);
        Assert.IsInstanceOfType(testProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(testProp.Value, typeof(PdxObject));

        var testObj = (PdxObject)testProp.Value;
        
        // Debug output - print all properties
        Console.WriteLine($"Test object has {testObj.Properties.Count} properties:");
        foreach (var prop in testObj.Properties)
        {
            string keyName = prop.Key is PdxString pdxString 
                ? pdxString.Value 
                : prop.Key.ToString() ?? "<null>";
                
            Console.WriteLine($"- Property: {keyName} (Type: {prop.Value.GetType().Name})");
        }
        
        Assert.AreEqual(5, testObj.Properties.Count);

        var intProp = FindProperty(testObj.Properties, "integer");
        Assert.IsInstanceOfType(intProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(intProp.Value, typeof(PdxInt));
        Assert.AreEqual(42, ((PdxInt)intProp.Value).Value);

        var flagProp = FindProperty(testObj.Properties, "flag");
        Assert.IsInstanceOfType(flagProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(flagProp.Value, typeof(PdxBool));
        Assert.IsTrue(((PdxBool)flagProp.Value).Value);

        var nestedProp = FindProperty(testObj.Properties, "nested");
        Assert.IsInstanceOfType(nestedProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(nestedProp.Value, typeof(PdxObject));

        var nestedObj = (PdxObject)nestedProp.Value;
        Assert.AreEqual(2, nestedObj.Properties.Count);

        var nameProp = FindProperty(nestedObj.Properties, "name");
        Assert.IsInstanceOfType(nameProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(nameProp.Value, typeof(PdxString));
        Assert.AreEqual("test", ((PdxString)nameProp.Value).Value);

        var valueProp = FindProperty(nestedObj.Properties, "value");
        Assert.IsInstanceOfType(valueProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(valueProp.Value, typeof(PdxFloat));
        Assert.AreEqual(3.14f, ((PdxFloat)valueProp.Value).Value);

        var arrayProp = FindProperty(testObj.Properties, "array");
        Assert.IsInstanceOfType(arrayProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(arrayProp.Value, typeof(PdxArray));

        var array = (PdxArray)arrayProp.Value;
        Assert.AreEqual(3, array.Items.Count);
        Assert.IsInstanceOfType(array.Items[0], typeof(PdxInt));
        Assert.AreEqual(1, ((PdxInt)array.Items[0]).Value);
        Assert.IsInstanceOfType(array.Items[1], typeof(PdxInt));
        Assert.AreEqual(2, ((PdxInt)array.Items[1]).Value);
        Assert.IsInstanceOfType(array.Items[2], typeof(PdxInt));
        Assert.AreEqual(3, ((PdxInt)array.Items[2]).Value);

        var dateProp = FindProperty(testObj.Properties, "date");
        Assert.IsInstanceOfType(dateProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(dateProp.Value, typeof(PdxDate));
        var date = (PdxDate)dateProp.Value;
        Assert.AreEqual(1960, date.Value.Year);
        Assert.AreEqual(1, date.Value.Month);
        Assert.AreEqual(1, date.Value.Day);
    }

    [TestMethod]
    public void Parse_Numeric_Keys()
    {
        // Arrange
        string input = """
            buildings = {
                101 = { 
                    id = 101
                    level = 2
                }
                102 = {
                    id = 102
                    level = 3
                }
            }
            """;

        // Act
        var result = PdxSaveReader.Read(input);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PdxObject));
        var obj = (PdxObject)result;
        
        var buildingsProp = FindProperty(obj.Properties, "buildings");
        Assert.IsInstanceOfType(buildingsProp.Key, typeof(PdxString));
        Assert.IsInstanceOfType(buildingsProp.Value, typeof(PdxObject));

        var buildingsObj = (PdxObject)buildingsProp.Value;
        Assert.AreEqual(2, buildingsObj.Properties.Count);

        // Find property with numeric key "101"
        var building101Prop = FindNumericProperty(buildingsObj.Properties, 101);
        Assert.IsNotNull(building101Prop.Value);
        Assert.IsInstanceOfType(building101Prop.Key, typeof(PdxInt));
        Assert.AreEqual(101, ((PdxInt)building101Prop.Key).Value);
        Assert.IsInstanceOfType(building101Prop.Value, typeof(PdxObject));

        var building101Obj = (PdxObject)building101Prop.Value;
        var idProp = FindProperty(building101Obj.Properties, "id");
        Assert.IsInstanceOfType(idProp.Value, typeof(PdxInt));
        Assert.AreEqual(101, ((PdxInt)idProp.Value).Value);

        // Find property with numeric key "102"
        var building102Prop = FindNumericProperty(buildingsObj.Properties, 102);
        Assert.IsNotNull(building102Prop.Value);
        Assert.IsInstanceOfType(building102Prop.Key, typeof(PdxInt));
        Assert.AreEqual(102, ((PdxInt)building102Prop.Key).Value);
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count);
        var termsProp = root.Properties[0];
        Assert.AreEqual("terms", ((PdxString)termsProp.Key).Value);

        Assert.IsInstanceOfType(termsProp.Value, typeof(PdxObject));
        var terms = (PdxObject)termsProp.Value;

        // Check discrete terms
        var discreteTermsProp = FindProperty(terms.Properties, "discrete_terms");
        Assert.IsInstanceOfType(discreteTermsProp.Value, typeof(PdxArray));
        var discreteTerms = (PdxArray)discreteTermsProp.Value;

        Assert.AreEqual(2, discreteTerms.Items.Count);

        // Check first discrete term
        var firstTermObj = (PdxObject)discreteTerms.Items[0];
        var firstTermKey = FindProperty(firstTermObj.Properties, "key");
        Assert.IsInstanceOfType(firstTermKey.Value, typeof(PdxString));
        Assert.AreEqual("specialist_type", ((PdxString)firstTermKey.Value).Value);

        var firstTermValue = FindProperty(firstTermObj.Properties, "value");
        Assert.IsInstanceOfType(firstTermValue.Value, typeof(PdxString));
        Assert.AreEqual("specialist_none", ((PdxString)firstTermValue.Value).Value);

        // Check second discrete term
        var secondTermObj = (PdxObject)discreteTerms.Items[1];
        var secondTermKey = FindProperty(secondTermObj.Properties, "key");
        Assert.IsInstanceOfType(secondTermKey.Value, typeof(PdxString));
        Assert.AreEqual("subject_integration", ((PdxString)secondTermKey.Value).Value);

        var secondTermValue = FindProperty(secondTermObj.Properties, "value");
        Assert.IsInstanceOfType(secondTermValue.Value, typeof(PdxString));
        Assert.AreEqual("subject_can_not_be_integrated", ((PdxString)secondTermValue.Value).Value);

        // Check resource terms
        var resourceTermsProp = FindProperty(terms.Properties, "resource_terms");
        Assert.IsInstanceOfType(resourceTermsProp.Value, typeof(PdxArray));
        var resourceTerms = (PdxArray)resourceTermsProp.Value;

        Assert.AreEqual(2, resourceTerms.Items.Count);

        // Check first resource term
        var firstResourceObj = (PdxObject)resourceTerms.Items[0];
        var firstResourceKey = FindProperty(firstResourceObj.Properties, "key");
        Assert.IsInstanceOfType(firstResourceKey.Value, typeof(PdxString));
        Assert.AreEqual("resource_subsidies_basic", ((PdxString)firstResourceKey.Value).Value);

        var firstResourceValue = FindProperty(firstResourceObj.Properties, "value");
        Assert.IsInstanceOfType(firstResourceValue.Value, typeof(PdxFloat));
        Assert.AreEqual(0.0f, ((PdxFloat)firstResourceValue.Value).Value);

        // Check second resource term
        var secondResourceObj = (PdxObject)resourceTerms.Items[1];
        var secondResourceKey = FindProperty(secondResourceObj.Properties, "key");
        Assert.IsInstanceOfType(secondResourceKey.Value, typeof(PdxString));
        Assert.AreEqual("resource_subsidies_advanced", ((PdxString)secondResourceKey.Value).Value);

        var secondResourceValue = FindProperty(secondResourceObj.Properties, "value");
        Assert.IsInstanceOfType(secondResourceValue.Value, typeof(PdxFloat));
        Assert.AreEqual(25.5f, ((PdxFloat)secondResourceValue.Value).Value);
    }

    [TestMethod]
    public void Parse_Empty_Object_Array()
    {
        // Arrange
        string input = """
                       culling_value={
                       	{ }
                       	{ }
                        { }
                       }
                       """;

        // Act
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count);
        var cullingValueProp = root.Properties[0];

        Assert.AreEqual("culling_value", ((PdxString)cullingValueProp.Key).Value);
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
                           intel_manager={
                               intel={
                                   {
                                       67108916 
                                       {
                                           intel=10
                                           stale_intel={
                                           }
                                       }
                                   }

                                   {
                                       218103860 
                                       {
                                           intel=10
                                           stale_intel={
                                           }
                                       }
                                   }
                               }
                           }
                       }
                       """;

        // Act
        var root = PdxSaveReader.Read(input.AsSpan());

        // Assert
        Assert.IsInstanceOfType(root, typeof(PdxObject));

        Assert.AreEqual(1, root.Properties.Count);
        var intelManagerProp = root.Properties[0];
        Assert.AreEqual("intel_manager", ((PdxString)intelManagerProp.Key).Value);

        Assert.IsInstanceOfType(intelManagerProp.Value, typeof(PdxObject));
        var intelManager = (PdxObject)intelManagerProp.Value;

        Assert.AreEqual(1, intelManager.Properties.Count);
        var intelProp = intelManager.Properties[0];
        Assert.AreEqual("intel", ((PdxString)intelProp.Key).Value);

        Assert.IsInstanceOfType(intelProp.Value, typeof(PdxArray));
        var intelArray = (PdxArray)intelProp.Value;

        Assert.AreEqual(2, intelArray.Items.Count);

        // Each item in the 'intel' array is itself an array [ID, DataObject]

        // Check first item
        Assert.IsInstanceOfType(intelArray.Items[0], typeof(PdxArray));
        var firstItemArray = (PdxArray)intelArray.Items[0];
        Assert.AreEqual(2, firstItemArray.Items.Count);

        // the first item in the array is an ID
        Assert.IsInstanceOfType(firstItemArray.Items[0], typeof(PdxInt));
        Assert.AreEqual(67108916, ((PdxInt)firstItemArray.Items[0]).Value);

        // the second item in the array is a data object
        Assert.IsInstanceOfType(firstItemArray.Items[1], typeof(PdxObject));

        var firstDataObject = (PdxObject)firstItemArray.Items[1];
        Assert.AreEqual(2, firstDataObject.Properties.Count); // intel and stale_intel

        var firstIntelValueProp = FindProperty(firstDataObject.Properties, "intel");
        Assert.IsInstanceOfType(firstIntelValueProp.Value, typeof(PdxInt));
        Assert.AreEqual(10, ((PdxInt)firstIntelValueProp.Value).Value);

        var firstStaleIntelProp = FindProperty(firstDataObject.Properties, "stale_intel");
        Assert.IsInstanceOfType(firstStaleIntelProp.Value, typeof(PdxObject));
        Assert.AreEqual(0, ((PdxObject)firstStaleIntelProp.Value).Properties.Count);


        // Check second item
        Assert.IsInstanceOfType(intelArray.Items[1], typeof(PdxArray)); // Second item is also an array [ID, DataObject]
        var secondItemArray = (PdxArray)intelArray.Items[1];
        Assert.AreEqual(2, secondItemArray.Items.Count); // Should contain ID and DataObject

        Assert.IsInstanceOfType(secondItemArray.Items[0], typeof(PdxInt));
        Assert.AreEqual(218103860, ((PdxInt)secondItemArray.Items[0]).Value);

        Assert.IsInstanceOfType(secondItemArray.Items[1], typeof(PdxObject));
        var secondDataObject = (PdxObject)secondItemArray.Items[1];
        Assert.AreEqual(2, secondDataObject.Properties.Count); // intel and stale_intel

        var secondIntelValueProp = FindProperty(secondDataObject.Properties, "intel");
        Assert.IsInstanceOfType(secondIntelValueProp.Value, typeof(PdxInt));
        Assert.AreEqual(10, ((PdxInt)secondIntelValueProp.Value).Value);

        var secondStaleIntelProp = FindProperty(secondDataObject.Properties, "stale_intel");
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
        var root = PdxSaveReader.Read(input.AsSpan());

        // Check the property values in the original PdxObject
        Assert.IsInstanceOfType(root, typeof(PdxObject));
        Assert.AreEqual(1, root.Properties.Count);

        Assert.IsTrue(root.TryGet<PdxObject>("test_record", out var testRecord));
        Assert.AreEqual(6, testRecord!.Properties.Count);

        // Verify we have 4 asteroid_postfix properties, each with array values
        var asteroidProps = new List<KeyValuePair<IPdxScalar, IPdxElement>>();

        foreach (var prop in testRecord.Properties)
        {
            if (prop.Key is PdxString pdxString && pdxString.Value == "asteroid_postfix")
            {
                asteroidProps.Add(prop);
            }
        }

        Assert.AreEqual(4, asteroidProps.Count, "Should have 4 asteroid_postfix properties");

        // Check the first asteroid_postfix property
        Assert.IsInstanceOfType(asteroidProps[0].Value, typeof(PdxArray));
        var firstArray = (PdxArray)asteroidProps[0].Value;
        Assert.AreEqual(2, firstArray.Items.Count);
        Assert.IsInstanceOfType(firstArray.Items[0], typeof(PdxString));
        Assert.AreEqual("413", ((PdxString)firstArray.Items[0]).Value);
        Assert.IsInstanceOfType(firstArray.Items[1], typeof(PdxString));
        Assert.AreEqual("3254", ((PdxString)firstArray.Items[1]).Value);
    }
}

