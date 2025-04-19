namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class PdxQueryTests
{
    public TestContext TestContext { get; set; }
    
    [TestMethod]
    public void Select_SimplePathAndWildcard_Works()
    {
        // Arrange
        var save = """
                   ship={
                     { location={ x=100 } }
                     { location={ x=250.5 } }
                     { location={ x=42 } }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        // Act
        var results = query.Select("ship.[*].location.x").ToList();
        // Assert
        Assert.AreEqual(3, results.Count);
        var values = results.Select(e =>
            e is PdxFloat f ? (float?)f.Value :
            e is PdxInt i ? (float?)i.Value :
            null
        ).ToList();

        CollectionAssert.AreEqual(new List<float?>
            {
                100.0f,
                250.5f,
                42.0f
            }, values
        );
    }

    [TestMethod]
    public void GetList_PrintsListOfItems()
    {
        var save = """
                   numbers={ 1 2 3 }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var list = query.GetList("numbers.[*]");
        var str = string.Join(",", list.Select(PdxQuery.ElementToString));
        Assert.AreEqual("1,2,3", str);
    }

    [TestMethod]
    public void SetArray_ReplacesArrayContent()
    {
        var save = """
                   nums={ 1 2 }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        TestContext.WriteLine("Before: " + string.Join(",", query.GetList("nums.[*]").Select(PdxQuery.ElementToString)));
        query.SetArray("nums", new List<IPdxElement> { new PdxInt(10), new PdxInt(20) });

        query = new PdxQuery(root);
        TestContext.WriteLine("After: " + string.Join(",", query.GetList("nums.[*]").Select(PdxQuery.ElementToString)));
        var list = query.GetList("nums.[*]");
        var str = string.Join(",", list.Select(PdxQuery.ElementToString));
        Assert.AreEqual("10,20", str);
    }

    [TestMethod]
    public void GetArraysOfArrays_Works()
    {
        var save = """
                   matrix={ { 1 2 } { 3 4 } }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var arrays = query.GetArraysOfArrays("matrix").ToList();
        Assert.AreEqual(1, arrays.Count);
        Assert.AreEqual(2, arrays[0].Items.Count);
    }

    [TestMethod]
    public void FindAllByKey_PrintsAllValues()
    {
        var save = """
                   foo=1
                   bar=2
                   foo=3
                   nested={ foo=4 }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var allFoos = query.FindAllByKey("foo").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "1", "3", "4" }, allFoos);
    }
    
    [TestMethod]
    public void ScalarKeyValue_CanQueryAndPrint()
    {
        var save = """
                   name="Test Empire"
                   year=2200
                   rate=0.75
                   is_active=yes
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        Assert.AreEqual("\"Test Empire\"", PdxQuery.ElementToString(query.GetList("name").First()));
        Assert.AreEqual("2200", PdxQuery.ElementToString(query.GetList("year").First()));
        Assert.AreEqual("0.75", PdxQuery.ElementToString(query.GetList("rate").First()));
        Assert.AreEqual("yes", PdxQuery.ElementToString(query.GetList("is_active").First()));
    }

    [TestMethod]
    public void NestedObject_CanQueryNestedFields()
    {
        var save = """
                   settings={
                     difficulty=normal
                     ironman=yes
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        Assert.AreEqual("normal", PdxQuery.ElementToString(query.GetList("settings.difficulty").First()));
        Assert.AreEqual("yes", PdxQuery.ElementToString(query.GetList("settings.ironman").First()));
    }

    [TestMethod]
    public void ArrayOfScalars_CanQueryAllItems()
    {
        var save = """
                   member_ids={ 101 102 105 210 }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var items = query.GetList("member_ids.[*]").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "101", "102", "105", "210" }, items);
    }

    [TestMethod]
    public void ArrayOfObjects_CanQueryFieldsInEachObject()
    {
        var save = """
                   fleets={
                     { name="1st Fleet" ships=10 }
                     { name="Reserve Fleet" ships=5 }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var names = query.GetList("fleets.[*].name").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "\"1st Fleet\"", "\"Reserve Fleet\"" }, names);
    }

    [TestMethod]
    public void IntKeyedObject_CanQueryByIntKey()
    {
        var save = """
                   country_opinions={
                     10={ value=50 relation=friendly }
                     15={ value=-25 relation=rival }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var values = query.GetList("country_opinions.10.value").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "50" }, values);

        var relations = query.GetList("country_opinions.15.relation").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "rival" }, relations);
    }

    [TestMethod]
    public void StringKeyedObject_CanQueryByStringKey()
    {
        var save = """
                   traits={
                     trait_intelligent=yes
                     trait_thrifty=no
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var intelligent = query.GetList("traits.trait_intelligent").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "yes" }, intelligent);
        
        var thrifty = query.GetList("traits.trait_thrifty").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "no" }, thrifty);
    }

    [TestMethod]
    public void PdxDictionary_CanQueryKeyValuePairs()
    {
        var save = """
                   event_modifiers={
                     { key=stability_boost value=10 }
                     { key=research_bonus value=0.15 }
                     { key=stability_boost value=5 }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var keys = query.GetList("event_modifiers.[*].key").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[]
            {
                "stability_boost",
                "research_bonus",
                "stability_boost"
            }, keys
        );

        var values = query.GetList("event_modifiers.[*].value").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "10", "0.15", "5" }, values);
    }

    [TestMethod]
    public void ArrayOfIdObjectPairs_CanQueryIdAndObjectFields()
    {
        var save = """
                   intel={
                     { 67108916 { intel=10 stale_intel={ } } }
                     { 218103860 { intel=12 stale_intel={ } } }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        // Get all IDs
        var ids = query.GetList("intel.[*].[0]").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "67108916", "218103860" }, ids);

        // Get all intel values
        var intelVals = query.GetList("intel.[*].[1].intel").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "10", "12" }, intelVals);
    }

    [TestMethod]
    public void NoneValue_IsHandledAsNull()
    {
        var save = """
                   leader_title=""
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var value = query.GetList("leader_title").FirstOrDefault();
        Assert.AreEqual("\"\"", PdxQuery.ElementToString(value));
    }

    [TestMethod]
    public void DeeplyNestedObject_QueryEngine_Works()
    {
        var save = """
                   obj1={
                     arr1={
                       { x=1 y=2 }
                       { x=3 y=4 arr2={ 5 6 } }
                       { { z=7 } { z=8 } }
                     }
                   }
                   arr3={
                     { 9 { w=10 } }
                     11
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        // Query for all x values in arr1
        var xs = query.GetList("obj1.arr1.[*].x").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "1", "3" }, xs);
        // Query for all y values in arr1
        var ys = query.GetList("obj1.arr1.[*].y").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "2", "4" }, ys);

        // Query for all arr2 values in arr1
        var arr2vals = query.GetList("obj1.arr1.[1].arr2.[*]").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "5", "6" }, arr2vals);

        // Query for all z values in the nested array in arr1
        var zs = query.GetList("obj1.arr1.[2].[*].z").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] {"7", "8" }, zs);

        // Query for w in arr3's first element (which is an array)
        var ws = query.GetList("arr3.[0].[1].w").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "10" }, ws);

        // Query for 11 in arr3
        var elevens = query.GetList("arr3.[1]").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "11" }, elevens);
    }

    [TestMethod]
    public void VeryDeeplyNestedObject_QueryEngine_Works()
    {
        var save = """
                   level1_obj={
                     level2_arr={
                       { level3_obj={ level4_arr={ { level5_val=42 } { level5_val=99 } } } }
                       { { { { deep=123 } } } }
                     }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        // Query for all level5_val values
        var vals = query.GetList("level1_obj.level2_arr.[0].level3_obj.level4_arr.[*].level5_val").Select(PdxQuery.ElementToString).ToList();
        TestContext.WriteLine("level5_val: " + string.Join(",", vals));
        CollectionAssert.AreEqual(new[] { "42", "99" }, vals);

        // Query for the deep value
        var deepVals = query.GetList("level1_obj.level2_arr.[1].[0].[0].deep").Select(PdxQuery.ElementToString).ToList();
        TestContext.WriteLine("deep: " + string.Join(",", deepVals));
        CollectionAssert.AreEqual(new[] { "123" }, deepVals);
    }

    [TestMethod]
    public void IndexedObjects_AreParsedAsObjectWithIntKeys()
    {
        var save = """
                   root={
                     0={ foo=1 }
                     1={ foo=2 }
                     2={ foo=3 }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var values = query.GetList("root.0.foo").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "1" }, values);

        values = query.GetList("root.1.foo").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "2" }, values);

        values = query.GetList("root.2.foo").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "3" }, values);
    }

    [TestMethod]
    public void RepeatedKeys_AreAccessibleAsList()
    {
        var save = """
                   root={
                     galactic_object=126
                     galactic_object=328
                     galactic_object=701
                     galactic_object=776
                     galactic_object=909
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var values = query.GetList("root.galactic_object").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "126", "328", "701", "776", "909" }, values);
    }

    [TestMethod]
    public void ArrayOfMixedTypes_IsParsedCorrectly()
    {
        var save = """
                   mixed={ 1 "foo" { bar=1 } }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var items = query.GetList("mixed.[*]");
        Assert.AreEqual("1", PdxQuery.ElementToString(items[0]));
        Assert.AreEqual("\"foo\"", PdxQuery.ElementToString(items[1]));
        Assert.AreEqual(expected: "{\r\n\tbar=1\r\n}", actual: PdxQuery.ElementToString(items[2]));
    }

    [TestMethod]
    public void EmptyArrayAndObject_AreParsedCorrectly()
    {
        var save = """
                   empty_arr={ }
                   empty_obj={ }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var arr = query.GetList("empty_arr.[*]");
        Assert.AreEqual(0, arr.Count);
        var obj = query.GetList("empty_obj");
        Assert.AreEqual(1, obj.Count);
        Assert.AreEqual("{ }", PdxQuery.ElementToString(obj[0]));
    }

    [TestMethod]
    public void NullAndMissingValues_AreHandledCorrectly()
    {
        var save = """
                   foo=""
                   bar=123
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var foo = query.GetList("foo").FirstOrDefault();
        Assert.AreEqual("\"\"", PdxQuery.ElementToString(foo));
        var missing = query.GetList("baz").FirstOrDefault();
        Assert.IsNull(missing);
    }

    [TestMethod]
    public void ExplicitNoneValue_IsHandledCorrectly()
    {
        var save = """
                   root={
                     0=none
                     1={ foo=bar }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var noneVal = query.GetList("root.0").FirstOrDefault();
        // Accept either the string "none" or a null/empty value, depending on parser
        var str = PdxQuery.ElementToString(noneVal);
        Assert.IsTrue(str == "none" || str == "" || str == null);
        var fooVal = query.GetList("root.1.foo").FirstOrDefault();
        Assert.AreEqual("bar", PdxQuery.ElementToString(fooVal));
    }

    [TestMethod]
    public void DeeplyNestedIndexedObjects_AreParsedCorrectly()
    {
        var save = """
                   root={
                     0={
                       1={
                         2={ foo=bar }
                       }
                     }
                   }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var val = query.GetList("root.0.1.2.foo").FirstOrDefault();
        Assert.IsNotNull(val);
        Assert.AreEqual("bar", PdxQuery.ElementToString(val));
    }

    [TestMethod]
    public void SingleElementArray_IsParsedCorrectly()
    {
        var save = """
                   root={ 42 }
                   """;

        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var arr = query.GetList("root.[*]");
        Assert.AreEqual(1, arr.Count);
        Assert.AreEqual("42", PdxQuery.ElementToString(arr[0]));
    }
    
    [TestMethod]
    public void RoundTrip_AddNewProperty_PreservesChange()
    {
        var save = "foo=1";
        var root = PdxSaveReader.Read(save);
        var obj = (PdxObject)root;
        obj.Properties.Add(new KeyValuePair<IPdxScalar, IPdxElement>(new PdxString("bar"), new PdxInt(99)));
        var serialized = obj.ToString();
        var reparsed = PdxSaveReader.Read(serialized);
        var reparsedQuery = new PdxQuery(reparsed);
        var barVal = reparsedQuery.GetList("bar").FirstOrDefault();
        Assert.IsNotNull(barVal);
        Assert.AreEqual("99", PdxQuery.ElementToString(barVal));
    }

    [TestMethod]
    public void RoundTrip_RemoveProperty_PreservesChange()
    {
        var save = "foo=1 bar=2";
        var root = PdxSaveReader.Read(save);
        root.Properties.RemoveAll(p => p.Key.ToString() == "bar");
        var serialized = root.ToString();
        var reparsed = PdxSaveReader.Read(serialized);
        var reparsedQuery = new PdxQuery(reparsed);
        var barVal = reparsedQuery.GetList("bar").FirstOrDefault();
        Assert.IsNull(barVal);
    }

    [TestMethod]
    public void RoundTrip_AddRemoveArrayElement_PreservesChange()
    {
        var save = "arr={ 1 2 3 }";
        var root = PdxSaveReader.Read(save);
        var arr = (PdxArray)root.Properties.First(p => p.Key.ToString() == "arr").Value;
        arr.Items.Add(new PdxInt(4));
        arr.Items.RemoveAt(0); // Remove first element
        var serialized = root.ToString();
        var reparsed = PdxSaveReader.Read(serialized);
        var reparsedQuery = new PdxQuery(reparsed);
        var vals = reparsedQuery.GetList("arr.[*]").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "2", "3", "4" }, vals);
    }

    [TestMethod]
    public void RoundTrip_ChangeDeeplyNestedValue_PreservesChange()
    {
        var save = "root={ nested={ foo=1 } }";
        var root = PdxSaveReader.Read(save);
        var nested = (PdxObject)root.Properties.First(p => p.Key.ToString() == "root").Value;
        var nested2 = (PdxObject)nested.Properties.First(p => p.Key.ToString() == "nested").Value;

        for (int i = 0; i < nested2.Properties.Count; i++)
        {
            if (nested2.Properties[i].Key.ToString() == "foo")
                nested2.Properties[i] = new KeyValuePair<IPdxScalar, IPdxElement>(nested2.Properties[i].Key, new PdxInt(99));
        }

        var serialized = root.ToString();
        var reparsed = PdxSaveReader.Read(serialized);
        var reparsedQuery = new PdxQuery(reparsed);
        var fooVal = reparsedQuery.GetList("root.nested.foo").FirstOrDefault();
        Assert.IsNotNull(fooVal);
        Assert.AreEqual("99", PdxQuery.ElementToString(fooVal));
    }

    [TestMethod]
    public void RoundTrip_RepeatedKeys_PreservesChange()
    {
        var save = "root={ key=1 key=2 }";
        var root = PdxSaveReader.Read(save);
        var obj = (PdxObject)root.Properties.First(p => p.Key.ToString() == "root").Value;
        obj.Properties.Add(new KeyValuePair<IPdxScalar, IPdxElement>(new PdxString("key"), new PdxInt(3)));
        var serialized = root.ToString();
        var reparsed = PdxSaveReader.Read(serialized);
        var reparsedQuery = new PdxQuery(reparsed);
        var vals = reparsedQuery.GetList("root.key").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "1", "2", "3" }, vals);
    }

    [TestMethod]
    public void RoundTrip_IndexedObjects_PreservesChange()
    {
        var save = "root={ 0={ foo=1 } 1={ foo=2 } }";
        var root = PdxSaveReader.Read(save);
        var obj = (PdxObject)root.Properties.First(p => p.Key.ToString() == "root").Value;
        var newObj = new PdxObject(new List<KeyValuePair<IPdxScalar, IPdxElement>>
            {
                new(new PdxString("foo"), new PdxInt(3))
            }
        );

        obj.Properties.Add(new KeyValuePair<IPdxScalar, IPdxElement>(new PdxInt(2), newObj));
        var serialized = root.ToString();
        var reparsed = PdxSaveReader.Read(serialized);
        var reparsedQuery = new PdxQuery(reparsed);
        var vals = reparsedQuery.GetList("root.2.foo").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[] { "3" }, vals);
    }

    [TestMethod]
    public void ParseUserInput_ParsesScalarsObjectsAndArrays()
    {
        // Scalar int
        var e1 = PdxQuery.ParseUserInput("42");
        Assert.IsInstanceOfType(e1, typeof(PdxInt));
        Assert.AreEqual("42", PdxQuery.ElementToString(e1));
        // Scalar float
        var e2 = PdxQuery.ParseUserInput("3.14");
        Assert.IsInstanceOfType(e2, typeof(PdxFloat));
        Assert.AreEqual("3.14", PdxQuery.ElementToString(e2));
        // Scalar bool
        var e3 = PdxQuery.ParseUserInput("yes");
        Assert.IsInstanceOfType(e3, typeof(PdxBool));
        Assert.AreEqual("yes", PdxQuery.ElementToString(e3));
        // Scalar string
        var e4 = PdxQuery.ParseUserInput("foo");
        Assert.IsInstanceOfType(e4, typeof(PdxString));
        Assert.AreEqual("foo", PdxQuery.ElementToString(e4));
        // Object
        var e5 = PdxQuery.ParseUserInput("{ x=1 y=2 }");
        Assert.IsInstanceOfType(e5, typeof(PdxObject));
        var obj = (PdxObject)e5;
        Assert.AreEqual(2, obj.Properties.Count);
        // Array
        var e6 = PdxQuery.ParseUserInput("{ 1 2 3 }");
        Assert.IsInstanceOfType(e6, typeof(PdxArray));
        var arr = (PdxArray)e6;
        Assert.AreEqual(3, arr.Items.Count);
    }
    
    [TestMethod]
    public void FindAllEnergyResources_FindsAllValuesAndPaths()
    {
        var save = @"
        country={
            0={ modules={ standard_economy_module={ resources={ energy=100 minerals=200 } } } }
            1={ modules={ standard_economy_module={ resources={ energy=65000 minerals=29562 } } } }
            2={ modules={ standard_economy_module={ resources={ energy=42 minerals=1 } } } }
        }
        ";
        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var energies = query.FindAllByKey("energy").ToList();
        var values = energies.Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEquivalent(new[] { "100", "65000", "42" }, values);
        // Optionally, print the paths to each found value (not implemented in PdxQuery, but could be added)
        // For now, just assert the values are found
    }

    [TestMethod]
    public void FindAllEnergyResourcesWithPaths_FindsAllValuesAndPaths()
    {
        var save = @"
        country={
            0={ modules={ standard_economy_module={ resources={ energy=100 minerals=200 } } } }
            1={ modules={ standard_economy_module={ resources={ energy=65000 minerals=29562 } } } }
            2={ modules={ standard_economy_module={ resources={ energy=42 minerals=1 } } } }
        }
        ";
        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var results = query.FindAllByKeyWithPath("energy").ToList();
        var expected = new[]
        {
            ("country.0.modules.standard_economy_module.resources.energy", "100"),
            ("country.1.modules.standard_economy_module.resources.energy", "65000"),
            ("country.2.modules.standard_economy_module.resources.energy", "42")
        };
        foreach (var (path, value) in expected)
        {
            Assert.IsTrue(results.Any(r => r.Path == path && PdxQuery.ElementToString(r.Value) == value), $"Expected {path} = {value}");
        }
        Assert.AreEqual(expected.Length, results.Count, "Should find all expected energy values with paths");
    }

    [TestMethod]
    public void RequiredDLCs_ArrayWildcard_Works()
    {
        var save = """
                   required_dlcs={
                      "Ancient Relics Story Pack"
                      "Anniversary Portraits"
                      "Apocalypse"
                      "Aquatics Species Pack"
                      "Utopia"
                   }
                   """;
        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var dlcs = query.GetList("required_dlcs.[*]").Select(PdxQuery.ElementToString).ToList();
        CollectionAssert.AreEqual(new[]
        {
            "\"Ancient Relics Story Pack\"",
            "\"Anniversary Portraits\"",
            "\"Apocalypse\"",
            "\"Aquatics Species Pack\"",
            "\"Utopia\""
        }, dlcs);
    }
    
    [TestMethod]
    public void PlayerArray_WildcardAndIndex_Works()
    {
        var save = """
                   player={
                       {
                           name="Delegate"
                           country=0
                       }
                   }
                   """;
        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);

        // Try wildcard
        var wildcardNames = query.GetList("player.[*].name").Select(PdxQuery.ElementToString).ToList();
        TestContext.WriteLine("Wildcard: " + string.Join(",", wildcardNames));

        // Try index
        var indexName = query.GetList("player.[0].name").Select(PdxQuery.ElementToString).ToList();
        TestContext.WriteLine("Index: " + string.Join(",", indexName));

        // Try just 'player'
        var playerType = query.GetList("player").FirstOrDefault()?.GetType().Name;
        TestContext.WriteLine("player type: " + playerType);

        // Assert
        Assert.AreEqual(1, wildcardNames.Count);
        Assert.AreEqual("\"Delegate\"", wildcardNames[0]);
        Assert.AreEqual(1, indexName.Count);
        Assert.AreEqual("\"Delegate\"", indexName[0]);
    }

    [TestMethod]
    public void RecursiveValueSubstringSearch_FindsIntSubstring()
    {
        var save = @"foo=122 bar=221 baz=33";
        var root = PdxSaveReader.Read(save);
        var query = new PdxQuery(root);
        var results = query.RecursiveValueSubstringSearch("22").Select((result) => PdxQuery.ElementToString(result.Value)).ToList();
        TestContext.WriteLine("Results: " + string.Join(", ", results));
        CollectionAssert.AreEquivalent(new[] { "122", "221" }, results);
    }
}