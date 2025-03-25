using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Test.Models;
namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

/// <summary>
/// Tests the source-generated binding functionality.
/// </summary>
[TestClass]
public class SourceGeneratedBinderTests
{
    [TestMethod]
    public void BindIndexedDictionary_WithIntKeys_Works()
    {
        // Arrange - Create input with 0=, 1=, 2= format like in Stellaris saves
        var input = """
                    scores={
                        0=42.5
                        1=37.8
                        2=99.1
                    }
                    """;
            
        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();
        
        // Act
        var model = DictionaryModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Scores);
        Assert.AreEqual(3, model.Scores.Count);
        Assert.AreEqual(42.5f, model.Scores[0]);
        Assert.AreEqual(37.8f, model.Scores[1]);
        Assert.AreEqual(99.1f, model.Scores[2]);
    }

    [TestMethod]
    public void BindIndexedDictionary_WithComplexValues_Works()
    {
        // Arrange - Create input with complex object values
        var input = """
                    resources={
                        0={
                            value=10
                            name="Test Resource"
                        }
                        1={
                            value=20
                            name="Another Resource"
                        }
                    }
                    """;

        var parser = new Parser.Parser(input);
        
        var saveObject = parser.Parse();
        
        // Act
        var model = DictionaryModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Resources);
        Assert.AreEqual(2, model.Resources.Count);
        Assert.AreEqual(10, model.Resources[0]!.Value);
        Assert.AreEqual("Test Resource", model.Resources[0]!.Name);
        Assert.AreEqual(20, model.Resources[1]!.Value);
        Assert.AreEqual("Another Resource", model.Resources[1]!.Name);
    }
    
    [TestMethod]
    public void SimpleTestModel_Bind_ReturnsCorrectValues()
    {
        // Arrange
        var obj = new SaveObject(
            [
                new("int_val", new Scalar<int>("int_val", 42)),
                new("string_val", new Scalar<string>("string_val", "hello")),
                new("date_value", new Scalar<DateOnly>("date_value", new DateOnly(2020, 01, 01))),
                new("array_value", new SaveArray([ 
                        new Scalar<int>("0", 1), 
                        new Scalar<int>("1", 2), 
                        new Scalar<int>("2", 3)]
                ))
            ]
        );

        // Act
        // Use the source-generated Bind method
        var result = TestModelForSourceGen.Bind(obj);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.IntValue);
        Assert.AreEqual("hello", result.StringValue);
        Assert.AreEqual(new DateOnly(2020, 01, 01), result.DateValue);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ArrayValue);
    }
    
    [TestMethod]
    public void TestSaveModel_WithDictionary_BindsCorrectly()
    {
        // Arrange
        var obj = new SaveObject(
            [
                new("dict_value", new SaveObject(
                    [
                        new("1", new Scalar<string>("1", "one")),
                        new("2", new Scalar<string>("2", "two")),
                        new("3", new Scalar<string>("3", "three"))
                    ])
                )
            ]
        );

        // Act
        // Use the source-generated Bind method
        var result = TestSaveModel.Bind(obj);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.DictValue!.Count);
        Assert.AreEqual("one", result.DictValue[1]);
        Assert.AreEqual("two", result.DictValue[2]);
        Assert.AreEqual("three", result.DictValue[3]);
        
    }
    
    [TestMethod]
    public void Bind_ListOfKeyValuePairs_ReturnsCorrectValues()
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

        var model = ModelWithDictionaryOfKeyValues.Bind(saveObject);

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
    public void ComplexNestedModel_Bind_CascadesCorrectly()
    {
        // Arrange - create a hierarchy of objects
        var nestedObj = new SaveObject(
            [
                new("name", new Scalar<string>("name", "nested")),
                new("value", new Scalar<int>("value", 123))
            ]
        );
        
        var arrayObjects = new SaveArray([
            new SaveObject([
                new("id", new Scalar<int>("id", 1)),
                new("description", new Scalar<string>("description", "first"))
            ]),
            new SaveObject([
                new("id", new Scalar<int>("id", 2)),
                new("description", new Scalar<string>("description", "second"))
            ])
        ]);
        
        var parentObj = new SaveObject(
            [
                new("name", new Scalar<string>("name", "parent")),
                new("nested_object", nestedObj),
                new("item_array", arrayObjects)
            ]
        );

        // Act
        // Use the source-generated Bind method
        var result = ParentModel.Bind(parentObj);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("parent", result.Name);
        
        // Verify nested object was bound correctly
        Assert.IsNotNull(result.NestedObject);
        Assert.AreEqual("nested", result.NestedObject.Name);
        Assert.AreEqual(123, result.NestedObject.Value);
        
        // Verify array of objects was bound correctly
        Assert.IsNotNull(result.ItemArray);
        Assert.AreEqual(2, result.ItemArray.Length);
        Assert.AreEqual(1, result.ItemArray[0].Id);
        Assert.AreEqual("first", result.ItemArray[0].Description);
        Assert.AreEqual(2, result.ItemArray[1].Id);
        Assert.AreEqual("second", result.ItemArray[1].Description);
    }
} 