using System.Text.Json;
using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Test.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

[TestClass]
public class ReflectionBinderTests : BindingTests
{
    [TestMethod]
    public void Dictionary_ReflectionBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateDictionaryTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<DictionaryModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Resources);
        Assert.AreEqual(2, result.Resources.Count);
        
        Assert.IsNotNull(result.Resources[1]);
        Assert.AreEqual("First Item", result.Resources[1]!.Name);
        Assert.AreEqual(100, result.Resources[1]!.Value);
        
        Assert.IsNotNull(result.Resources[2]);
        Assert.AreEqual("Second Item", result.Resources[2]!.Name);
        Assert.AreEqual(200, result.Resources[2]!.Value);
        
        // Check scores
        Assert.IsNotNull(result.Scores);
        Assert.AreEqual(2, result.Scores.Count);
        Assert.AreEqual(42.5f, result.Scores[1]);
        Assert.AreEqual(99.9f, result.Scores[2]);
    }
    
    [TestMethod]
    public void Array_Binding()
    {
        // Arrange
        var saveObject = CreateArrayTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<SimpleTestModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ArrayValue);
        Assert.AreEqual(3, result.ArrayValue.Length);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ArrayValue);
    }
    
    [TestMethod]
    public void RepeatedProperties_ReflectionBinding_CollectsIntoArray()
    {
        // Arrange
        var saveObject = CreateRepeatedPropertiesTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<RepeatedPropertyModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Sections);
        Assert.AreEqual(4, result.Sections.Count);
        
        Assert.AreEqual("SECTION_1", result.Sections[0]!.Design);
        Assert.AreEqual("1", result.Sections[0]!.Slot);
        
        Assert.AreEqual("SECTION_2", result.Sections[1]!.Design);
        Assert.AreEqual("2", result.Sections[1]!.Slot);
        
        // The 3rd section is 'none', which is included by the ReflectionBinder
        
        Assert.AreEqual("SECTION_3", result.Sections[3]!.Design);
        Assert.AreEqual("3", result.Sections[3]!.Slot);
    }
    
    [TestMethod]
    public void List_ReflectionBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateListTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<ListModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Values);
        Assert.AreEqual(3, result.Values.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.Values.ToArray());
    }
    
    [TestMethod]
    public void Bind_NestedObject()
    {
        // Arrange
        var saveObject = CreateNestedObjectTestData();
        
        // Act
        var result = ReflectionBinder.Bind<ParentModel>(saveObject);
        
        // Assert
        AssertNestedObjectBoundCorrectly(result!);
    }
    
    [TestMethod]
    public void Bind_Dictionary()
    {
        // Arrange
        var saveObject = CreateDictionaryTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<TestSaveModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.DictValue!.Count);
        Assert.AreEqual("one", result.DictValue[1]);
        Assert.AreEqual("two", result.DictValue[2]);
        Assert.AreEqual("three", result.DictValue[3]);
    }
     
    [TestMethod]
    public void Bind_SimpleValues()
    {
        // Arrange
        var saveObject = CreateSimpleTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<TestModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Empire", result.Name);
        Assert.AreEqual(5, result.Capital);
        Assert.AreEqual(new DateOnly(2200, 1, 1), result.StartDate);
        Assert.IsTrue(result.Ironman);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.Achievements);
        Assert.AreEqual(new Guid("12345678-1234-5678-1234-567812345678"), result.Id);
    }
    
    [TestMethod]
    public void Bind_ListOfKeyValuePairs()
    {
        /*
         *  bind what appears to be a "array of strings" but is actually a dictionary of key-value pairs
         *  In this example,
         *  From a Parser perspective, this is an array of KeyValuePair<<Scalar<string>, Scalar<int>>
         *  From a Model binding perspective, this is a Dictionary<string, int>
         */
        
        string input = """
            items={
                "item1"=1
                "item2"=1
                "%SEQ%"=16
                "item3"=0
                "item4"=1
            }
        """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();

        var model = ReflectionBinder.Bind<ModelWithDictionaryOfKeyValues>(saveObject);

        Assert.IsNotNull(model);
        Assert.AreEqual(5, model.Items!.Count);
        
        Assert.IsTrue(model.Items.ContainsKey("item1"));
        Assert.IsTrue(model.Items.ContainsKey("item2"));
        Assert.IsTrue(model.Items.ContainsKey("%SEQ%"));
        Assert.IsTrue(model.Items.ContainsKey("item3"));
        Assert.IsTrue(model.Items.ContainsKey("item4"));
        
        Assert.AreEqual(1, model.Items["item1"]);
        Assert.AreEqual(1, model.Items["item2"]);
        Assert.AreEqual(16, model.Items["%SEQ%"]);
        Assert.AreEqual(0, model.Items["item3"]);
        Assert.AreEqual(1, model.Items["item4"]);
    }
    
    [TestMethod]
    public void Bind_SimpleProperties()
    {
        string input = """
            name="Test Empire"
            capital=5
            start_date="2200.01.01"
            ironman=yes
            achievement={ 1 2 3 }
            id="12345678-1234-5678-1234-567812345678"
        """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();

        var model = ReflectionBinder.Bind<TestModel>(saveObject);

        Assert.AreEqual("Test Empire", model!.Name);
        Assert.AreEqual(5, model.Capital);
        Assert.AreEqual(new DateOnly(2200, 1, 1), model.StartDate);
        Assert.IsTrue(model.Ironman);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, model.Achievements);
        Assert.AreEqual(new Guid("12345678-1234-5678-1234-567812345678"), model.Id);
    }

    [TestMethod]
    public void Bind_ComplexStructure()
    {
        string input = """
            name="Galactic Empire"
            capital=42
            resources={ 
                energy=500 
                minerals=1000 
                name="Main Hub"
                efficiency=0.75
            }
            planets={
                { 
                    energy=100 
                    minerals=200
                    name="Colony 1"
                    efficiency=0.85
                }
                { 
                    energy=300 
                    minerals=400
                    name="Colony 2"
                    efficiency=0.95
                }
            }
            values={ 1 2.5 3 4.75 }
            tags={ "alpha" "beta" "gamma" }
            enabled=yes
            disabled=no
            start_date="2200.01.01"
            nested={
                energy=50
                minerals=75
                name="Nested Hub"
                efficiency=0.65
            }
        """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();

        var model = ReflectionBinder.Bind<ComplexModel>(saveObject);

        Assert.AreEqual("Galactic Empire", model!.Name);
        Assert.AreEqual(42, model.Capital);
        
        // Check resources
        Assert.AreEqual(500, model.Resources!.Energy);
        Assert.AreEqual(1000, model.Resources.Minerals);
        Assert.AreEqual("Main Hub", model.Resources.Name);
        Assert.AreEqual(0.75f, model.Resources.Efficiency);

        // Check planets
        Assert.AreEqual(2, model.Planets!.Count);
        
        Assert.AreEqual(100, model.Planets[0].Energy);
        Assert.AreEqual(200, model.Planets[0].Minerals);
        Assert.AreEqual("Colony 1", model.Planets[0].Name);
        Assert.AreEqual(0.85f, model.Planets[0].Efficiency);
        
        Assert.AreEqual(300, model.Planets[1].Energy);
        Assert.AreEqual(400, model.Planets[1].Minerals);
        Assert.AreEqual("Colony 2", model.Planets[1].Name);
        Assert.AreEqual(0.95f, model.Planets[1].Efficiency);

        // Check arrays
        CollectionAssert.AreEqual(new[] { 1f, 2.5f, 3f, 4.75f }, model.Values);
        CollectionAssert.AreEqual(new[] { "alpha", "beta", "gamma" }, model.Tags);

        // Check booleans
        Assert.IsTrue(model.Enabled);
        Assert.IsFalse(model.Disabled);

        // Check date
        Assert.AreEqual(new DateOnly(2200, 1, 1), model.StartDate);

        // Check nested object
        Assert.AreEqual(50, model.Nested!.Energy);
        Assert.AreEqual(75, model.Nested.Minerals);
        Assert.AreEqual("Nested Hub", model.Nested.Name);
        Assert.AreEqual(0.65f, model.Nested.Efficiency);
    }

    [TestMethod]
    public void Bind_IndexedCollection()
    {
        var input = """
            exhibits={
                1={
                    exhibit_state="active"
                    specimen={
                        id="spec_001"
                        origin="earth"
                    }
                    owner="museum_1"
                    date_added="2300.1.1"
                }
                2={
                    exhibit_state="inactive"
                    specimen={
                        id="spec_002"
                        origin="mars"
                    }
                    owner="museum_2"
                    date_added="2300.2.1"
                }
            }
            """;

        var parser = new Parser.Parser(input);
        var root = parser.Parse();

        var result = ReflectionBinder.Bind<ExhibitsContainer>(root);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Exhibits!.Count);

        var exhibit1 = result.Exhibits[1];
        Assert.AreEqual("active", exhibit1.State);
        Assert.IsNotNull(exhibit1.Specimen);
        Assert.AreEqual("spec_001", exhibit1.Specimen.Id);
        Assert.AreEqual("earth", exhibit1.Specimen.Origin);
        Assert.AreEqual("museum_1", exhibit1.Owner);
        Assert.AreEqual(new DateOnly(2300, 1, 1), exhibit1.DateAdded);

        var exhibit2 = result.Exhibits[2];
        Assert.AreEqual("inactive", exhibit2.State);
        Assert.IsNotNull(exhibit2.Specimen);
        Assert.AreEqual("spec_002", exhibit2.Specimen.Id);
        Assert.AreEqual("mars", exhibit2.Specimen.Origin);
        Assert.AreEqual("museum_2", exhibit2.Owner);
        Assert.AreEqual(new DateOnly(2300, 2, 1), exhibit2.DateAdded);
    }

    [TestMethod]
    public void Bind_DuplicateProperties()
    {
        var input = """
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
            """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();
        var result = ReflectionBinder.Bind<ShipData>(saveObject);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Sections!.Count);

        // Check first section
        var section1 = result.Sections[0];
        Assert.AreEqual("STARHOLD_STARBASE_SECTION", section1.Design);
        Assert.AreEqual("core", section1.Slot);
        Assert.AreEqual(2, section1.Weapons!.Count);

        var weapon1 = section1.Weapons[0];
        Assert.AreEqual(47, weapon1.Index);
        Assert.AreEqual("MEDIUM_MASS_DRIVER_1", weapon1.Template);
        Assert.AreEqual("MEDIUM_GUN_01", weapon1.ComponentSlot);

        var weapon2 = section1.Weapons[1];
        Assert.AreEqual(48, weapon2.Index);
        Assert.AreEqual("MEDIUM_MASS_DRIVER_1", weapon2.Template);
        Assert.AreEqual("MEDIUM_GUN_02", weapon2.ComponentSlot);

        // Check second section
        var section2 = result.Sections[1];
        Assert.AreEqual("ASSEMBLYYARD_STARBASE_SECTION", section2.Design);
        Assert.AreEqual("1", section2.Slot);
        Assert.AreEqual(0, section2.Weapons!.Count);

        // Check third section
        var section3 = result.Sections[2];
        Assert.AreEqual("REFINERY_STARBASE_SECTION", section3.Design);
        Assert.AreEqual("2", section3.Slot);
        Assert.AreEqual(0, section3.Weapons!.Count);
    }

    [TestMethod]
    public void Bind_RepeatedProperties()
    {
        var input = """
            section={
                design="SECTION_1"
                slot="1"
            }
            section={
                design="SECTION_2"
                slot="2"
            }
            section=none
            section={
                design="SECTION_3"
                slot="3"
            }
            """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();

        // Debug output
        Console.WriteLine("SaveObject properties:");
        foreach (var prop in saveObject.Properties)
        {
            Console.WriteLine($"Key: {prop.Key}, Value type: {prop.Value.GetType().Name}");
            if (prop.Value is SaveObject so)
            {
                foreach (var innerProp in so.Properties)
                {
                    Console.WriteLine($"  Inner Key: {innerProp.Key}, Value type: {innerProp.Value.GetType().Name ?? "null"}");
                }
            }
        }

        var result = ReflectionBinder.Bind<RepeatedPropertyModel>(saveObject);

        Assert.IsNotNull(result, "Result should not be null");
        Assert.IsNotNull(result.Sections, "Sections should not be null");
        Assert.AreEqual(4, result.Sections.Count, "Should have 4 sections");

        // First section
        Assert.IsNotNull(result.Sections[0], "First section should not be null");
        Assert.AreEqual("SECTION_1", result.Sections[0]!.Design, "First section design mismatch");
        Assert.AreEqual("1", result.Sections[0]!.Slot, "First section slot mismatch");

        // Second section
        Assert.IsNotNull(result.Sections[1], "Second section should not be null");
        Assert.AreEqual("SECTION_2", result.Sections[1]!.Design, "Second section design mismatch");
        Assert.AreEqual("2", result.Sections[1]!.Slot, "Second section slot mismatch");

        // Third section (none)
        Assert.IsNull(result.Sections[2], "Third section should be null");

        // Fourth section
        Assert.IsNotNull(result.Sections[3], "Fourth section should not be null");
        Assert.AreEqual("SECTION_3", result.Sections[3]!.Design, "Fourth section design mismatch");
        Assert.AreEqual("3", result.Sections[3]!.Slot, "Fourth section slot mismatch");
    }

    [TestMethod]
    public void Bind_List()
    {
        var input = """
            values={ 1 2 3 4 5 }
            strings={ "alpha" "beta" "gamma" }
            nested={
                {
                    energy=100
                    minerals=200
                    name="Resource 1"
                    efficiency=0.75
                }
                {
                    energy=300
                    minerals=400
                    name="Resource 2"
                    efficiency=0.85
                }
            }
            """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();
        var result = ReflectionBinder.Bind<ListModel>(saveObject);

        // Check that we got ImmutableList instances
        Assert.IsInstanceOfType(result!.Values, typeof(List<int>));
        Assert.IsInstanceOfType(result.Strings, typeof(List<string>));
        Assert.IsInstanceOfType(result.Nested, typeof(List<NestedModel>));

        // Check values
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result.Values.ToArray());
        CollectionAssert.AreEqual(new[] { "alpha", "beta", "gamma" }, result.Strings.ToArray());

        // Check resources
        Assert.AreEqual(2, result.Nested.Count);
        
        var resource1 = result.Nested[0]!;
        Assert.AreEqual(100, resource1.Energy);
        Assert.AreEqual(200, resource1.Minerals);
        Assert.AreEqual("Resource 1", resource1.Name);
        Assert.AreEqual(0.75f, resource1.Efficiency);

        var resource2 = result.Nested[1]!;
        Assert.AreEqual(300, resource2.Energy);
        Assert.AreEqual(400, resource2.Minerals);
        Assert.AreEqual("Resource 2", resource2.Name);
        Assert.AreEqual(0.85f, resource2.Efficiency);
    }
}