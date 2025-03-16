using Microsoft.VisualStudio.TestTools.UnitTesting;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Shared;

[TestClass]
public class BinderTests
{
    public class TestModel
    {
        [SaveScalar("name")]
        public string Name { get; set; } = string.Empty;

        [SaveScalar("capital")]
        public int Capital { get; set; }

        [SaveScalar("start_date")]
        public DateOnly StartDate { get; set; }

        [SaveArray("achievement")]
        public ImmutableList<int> Achievements { get; set; } = [];

        [SaveScalar("ironman")]
        public bool Ironman { get; set; }

        [SaveScalar("id")]
        public Guid Id { get; set; }
    }

    public class NestedModel
    {
        [SaveScalar("energy")]
        public int Energy { get; init; }

        [SaveScalar("minerals")]
        public int Minerals { get; init; }

        [SaveScalar("name")]
        public string Name { get; init; }

        [SaveScalar("efficiency")]
        public float Efficiency { get; init; }
    }

    public class ComplexModel
    {
        [SaveScalar("name")]
        public string Name { get; set; } = string.Empty;

        [SaveScalar("capital")]
        public int Capital { get; set; }

        [SaveObject("resources")]
        public NestedModel Resources { get; set; } = new();

        [SaveArray("planets")]
        public List<NestedModel> Planets { get; set; } = new();

        [SaveArray("values")]
        public float[] Values { get; set; } = [];

        [SaveArray("tags")]
        public string[] Tags { get; set; } = [];

        [SaveScalar("enabled")]
        public bool Enabled { get; set; }

        [SaveScalar("disabled")]
        public bool Disabled { get; set; }

        [SaveScalar("start_date")]
        public DateOnly StartDate { get; set; }

        [SaveObject("nested")]
        public NestedModel Nested { get; set; } = new();
    }

    private class ExhibitSpecimen
    {
        [SaveScalar("id")]
        public string Id { get; set; } = "";

        [SaveScalar("origin")]
        public string Origin { get; set; } = "";
    }

    private class Exhibit
    {
        [SaveScalar("exhibit_state")]
        public string State { get; set; } = "";

        [SaveObject("specimen")]
        public ExhibitSpecimen? Specimen { get; set; }

        [SaveScalar("owner")]
        public string Owner { get; set; } = "";

        [SaveScalar("date_added")]
        public DateOnly DateAdded { get; set; }
    }

    private class ExhibitsContainer
    {
        [SaveIndexedDictionary("exhibits")]
        public ImmutableDictionary<int, Exhibit> Exhibits { get; set; }
    }

    private class WeaponData
    {
        [SaveScalar("index")]
        public int Index { get; set; }

        [SaveScalar("template")]
        public string Template { get; set; } = "";

        [SaveScalar("component_slot")]
        public string ComponentSlot { get; set; } = "";
    }

    private class SectionData
    {
        [SaveScalar("design")]
        public string Design { get; set; } = "";

        [SaveScalar("slot")]
        public string Slot { get; set; } = "";

        [SaveArray("weapon")]
        public WeaponData[] Weapons { get; set; }
    }

    private class ShipData
    {
        [SaveArray("section")]
        public ImmutableArray<SectionData> Sections { get; set; }
    }

    private class RepeatedPropertyModel
    {
        [SaveArray("section")]
        public ImmutableList<SectionData> Sections { get; set; } = [];
    }

    private class ImmutableListModel
    {
        [SaveArray("values")]
        public ImmutableList<int> Values { get; init; }

        [SaveArray("strings")]
        public ImmutableList<string> Strings { get; init; }

        [SaveArray("nested")]
        public ImmutableList<NestedModel> Nested { get; init; }
    }

    private class ImmutableDictionaryModel
    {
        [SaveObject("resources")]
        public ImmutableDictionary<string, NestedModel> Resources { get; init; }

        [SaveObject("scores")]
        public ImmutableDictionary<int, float> Scores { get; init; }
    }

    [TestMethod]
    public void Bind_SimpleProperties_ReturnsCorrectValues()
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

        var model = Binder.Bind<TestModel>(saveObject);

        Assert.AreEqual("Test Empire", model.Name);
        Assert.AreEqual(5, model.Capital);
        Assert.AreEqual(new DateOnly(2200, 1, 1), model.StartDate);
        Assert.IsTrue(model.Ironman);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, model.Achievements);
        Assert.AreEqual(new Guid("12345678-1234-5678-1234-567812345678"), model.Id);
    }

    [TestMethod]
    public void Bind_ComplexStructure_ReturnsCorrectValues()
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

        var model = Binder.Bind<ComplexModel>(saveObject);

        Assert.AreEqual("Galactic Empire", model.Name);
        Assert.AreEqual(42, model.Capital);
        
        // Check resources
        Assert.AreEqual(500, model.Resources.Energy);
        Assert.AreEqual(1000, model.Resources.Minerals);
        Assert.AreEqual("Main Hub", model.Resources.Name);
        Assert.AreEqual(0.75f, model.Resources.Efficiency);

        // Check planets
        Assert.AreEqual(2, model.Planets.Count);
        
        Assert.AreEqual(100, model.Planets[0].Energy);
        Assert.AreEqual(200, model.Planets[0].Minerals);
        Assert.AreEqual("Colony 1", model.Planets[0].Name);
        Assert.AreEqual(0.85f, model.Planets[0].Efficiency);
        
        Assert.AreEqual(300, model.Planets[1].Energy);
        Assert.AreEqual(400, model.Planets[1].Minerals);
        Assert.AreEqual("Colony 2", model.Planets[1].Name);
        Assert.AreEqual(0.95f, model.Planets[1].Efficiency);

        // Check arrays
        CollectionAssert.AreEqual(new float[] { 1f, 2.5f, 3f, 4.75f }, model.Values);
        CollectionAssert.AreEqual(new string[] { "alpha", "beta", "gamma" }, model.Tags);

        // Check booleans
        Assert.IsTrue(model.Enabled);
        Assert.IsFalse(model.Disabled);

        // Check date
        Assert.AreEqual(new DateOnly(2200, 1, 1), model.StartDate);

        // Check nested object
        Assert.AreEqual(50, model.Nested.Energy);
        Assert.AreEqual(75, model.Nested.Minerals);
        Assert.AreEqual("Nested Hub", model.Nested.Name);
        Assert.AreEqual(0.65f, model.Nested.Efficiency);
    }

    [TestMethod]
    public void Bind_IndexedCollection_ReturnsCorrectValues()
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
                    date_added=2300.1.1
                }
                2={
                    exhibit_state="inactive"
                    specimen={
                        id="spec_002"
                        origin="mars"
                    }
                    owner="museum_2"
                    date_added=2300.2.1
                }
            }
            """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();
        var result = Binder.Bind<ExhibitsContainer>(saveObject);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Exhibits.Count);

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
    public void Bind_DuplicateProperties_ReturnsCorrectStructure()
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
        var result = Binder.Bind<ShipData>(saveObject);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Sections.Length);

        // Check first section
        var section1 = result.Sections[0];
        Assert.AreEqual("STARHOLD_STARBASE_SECTION", section1.Design);
        Assert.AreEqual("core", section1.Slot);
        Assert.AreEqual(2, section1.Weapons.Length);

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
        Assert.AreEqual(0, section2.Weapons.Length);

        // Check third section
        var section3 = result.Sections[2];
        Assert.AreEqual("REFINERY_STARBASE_SECTION", section3.Design);
        Assert.AreEqual("2", section3.Slot);
        Assert.AreEqual(0, section3.Weapons.Length);
    }

    [TestMethod]
    public void Bind_RepeatedProperties_ReturnsCorrectCollection()
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
            Console.WriteLine($"Key: {prop.Key}, Value type: {prop.Value?.GetType().Name ?? "null"}");
            if (prop.Value is SaveObject so)
            {
                foreach (var innerProp in so.Properties)
                {
                    Console.WriteLine($"  Inner Key: {innerProp.Key}, Value type: {innerProp.Value?.GetType().Name ?? "null"}");
                }
            }
        }

        var result = Binder.Bind<RepeatedPropertyModel>(saveObject);

        Assert.IsNotNull(result, "Result should not be null");
        Assert.IsNotNull(result.Sections, "Sections should not be null");
        Assert.AreEqual(4, result.Sections.Count, "Should have 4 sections");

        // First section
        Assert.IsNotNull(result.Sections[0], "First section should not be null");
        Assert.AreEqual("SECTION_1", result.Sections[0].Design, "First section design mismatch");
        Assert.AreEqual("1", result.Sections[0].Slot, "First section slot mismatch");

        // Second section
        Assert.IsNotNull(result.Sections[1], "Second section should not be null");
        Assert.AreEqual("SECTION_2", result.Sections[1].Design, "Second section design mismatch");
        Assert.AreEqual("2", result.Sections[1].Slot, "Second section slot mismatch");

        // Third section (none)
        Assert.IsNotNull(result.Sections[2], "Third section should not be null");
        Assert.AreEqual("", result.Sections[2].Design, "Third section design should be empty");
        Assert.AreEqual("", result.Sections[2].Slot, "Third section slot should be empty");

        // Fourth section
        Assert.IsNotNull(result.Sections[3], "Fourth section should not be null");
        Assert.AreEqual("SECTION_3", result.Sections[3].Design, "Fourth section design mismatch");
        Assert.AreEqual("3", result.Sections[3].Slot, "Fourth section slot mismatch");
    }

    [TestMethod]
    public void Bind_ImmutableList_ReturnsCorrectValues()
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
        var result = Binder.Bind<ImmutableListModel>(saveObject);

        // Check that we got ImmutableList instances
        Assert.IsInstanceOfType(result.Values, typeof(ImmutableList<int>));
        Assert.IsInstanceOfType(result.Strings, typeof(ImmutableList<string>));
        Assert.IsInstanceOfType(result.Nested, typeof(ImmutableList<NestedModel>));

        // Check values
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, result.Values.ToArray());
        CollectionAssert.AreEqual(new[] { "alpha", "beta", "gamma" }, result.Strings.ToArray());

        // Check resources
        Assert.AreEqual(2, result.Nested.Count);
        
        var resource1 = result.Nested[0];
        Assert.AreEqual(100, resource1.Energy);
        Assert.AreEqual(200, resource1.Minerals);
        Assert.AreEqual("Resource 1", resource1.Name);
        Assert.AreEqual(0.75f, resource1.Efficiency);

        var resource2 = result.Nested[1];
        Assert.AreEqual(300, resource2.Energy);
        Assert.AreEqual(400, resource2.Minerals);
        Assert.AreEqual("Resource 2", resource2.Name);
        Assert.AreEqual(0.85f, resource2.Efficiency);

        // Verify immutability
        Assert.ThrowsException<NotSupportedException>(() => 
        {
            var list = result.Values as IList<int>;
            list!.Add(6);
        });
    }

    [TestMethod]
    public void Bind_ImmutableDictionary_ReturnsCorrectValues()
    {
        var input = """
            resources={
                alpha={
                    energy=100
                    minerals=200
                    name="Resource Alpha"
                    efficiency=0.75
                }
                beta={
                    energy=300
                    minerals=400
                    name="Resource Beta"
                    efficiency=0.85
                }
            }
            scores={
                1=3.14
                2=6.28
                3=9.42
            }
            """;

        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();
        var result = Binder.Bind<ImmutableDictionaryModel>(saveObject);

        // Check that we got ImmutableDictionary instances
        Assert.IsInstanceOfType(result.Resources, typeof(ImmutableDictionary<string, NestedModel>));
        Assert.IsInstanceOfType(result.Scores, typeof(ImmutableDictionary<int, float>));

        // Check resources
        Assert.AreEqual(2, result.Resources.Count);
        
        var resourceAlpha = result.Resources["alpha"];
        Assert.AreEqual(100, resourceAlpha.Energy);
        Assert.AreEqual(200, resourceAlpha.Minerals);
        Assert.AreEqual("Resource Alpha", resourceAlpha.Name);
        Assert.AreEqual(0.75f, resourceAlpha.Efficiency);

        var resourceBeta = result.Resources["beta"];
        Assert.AreEqual(300, resourceBeta.Energy);
        Assert.AreEqual(400, resourceBeta.Minerals);
        Assert.AreEqual("Resource Beta", resourceBeta.Name);
        Assert.AreEqual(0.85f, resourceBeta.Efficiency);

        // Check scores
        Assert.AreEqual(3, result.Scores.Count);
        Assert.AreEqual(3.14f, result.Scores[1]);
        Assert.AreEqual(6.28f, result.Scores[2]);
        Assert.AreEqual(9.42f, result.Scores[3]);

        // Verify immutability
        Assert.ThrowsException<NotSupportedException>(() => 
        {
            var dict = result.Resources as IDictionary<string, NestedModel>;
            dict!.Add("gamma", new NestedModel());
        });
    }
}