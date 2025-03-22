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
        Assert.AreEqual(3, result.DictValue.Count);
        Assert.AreEqual("one", result.DictValue[1]);
        Assert.AreEqual("two", result.DictValue[2]);
        Assert.AreEqual("three", result.DictValue[3]);
        
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