using System.Text.Json;
using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Test.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

public static class Category
{
    public const string ReflectionBinding = "ReflectionBinding";
    public const string SourceGeneratedBinding = "SourceGeneratedBinding";
}

/// <summary>
/// Unified tests for binding functionality (both reflection-based and source-generated).
/// </summary>
[TestClass]
public class BindingTests
{
    #region Simple Value Tests
    
    [TestMethod]
    [TestCategory(Category.ReflectionBinding)]
    public void SimpleValues_ReflectionBinding_ReturnsCorrectValues()
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
    [TestCategory(Category.SourceGeneratedBinding)]
    public void SimpleValues_SourceGenBinding_ReturnsCorrectValues()
    {
        // Arrange
        var saveObject = CreateSimpleTestObject();
        
        // Act
        var result = TestModelForSourceGen.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.IntValue);
        Assert.AreEqual("hello", result.StringValue);
        Assert.AreEqual(new DateOnly(2020, 01, 01), result.DateValue);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ArrayValue);
    }
    
    #endregion
    
    #region Dictionary Tests
    
    [TestMethod]
    [TestCategory(Category.ReflectionBinding)]
    public void Dictionary_ReflectionBinding_BindsCorrectly()
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
    [TestCategory(Category.SourceGeneratedBinding)]
    public void Dictionary_SourceGenBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateDictionaryTestObject();
        
        // Act
        var result = TestSaveModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.DictValue!.Count);
        Assert.AreEqual("one", result.DictValue[1]);
        Assert.AreEqual("two", result.DictValue[2]);
        Assert.AreEqual("three", result.DictValue[3]);
    }
    
    [TestMethod]
    [TestCategory(Category.SourceGeneratedBinding)]
    [TestCategory(Category.ReflectionBinding)]
    public void Dictionary_BothBindingApproaches_ProduceIdenticalResults()
    {
        // Arrange
        var saveObject = CreateDictionaryTestObject();
        
        // Act
        var reflectionResult = ReflectionBinder.Bind<TestSaveModel>(saveObject);
        var sourceGenResult = TestSaveModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(reflectionResult);
        Assert.IsNotNull(sourceGenResult);
        Assert.AreEqual(reflectionResult.DictValue!.Count, sourceGenResult.DictValue!.Count);
        
        foreach (var key in reflectionResult.DictValue.Keys)
        {
            Assert.IsTrue(sourceGenResult.DictValue.ContainsKey(key));
            Assert.AreEqual(reflectionResult.DictValue[key], sourceGenResult.DictValue[key]);
        }
    }
    
    #endregion
    
    #region Nested Object Tests
    
    [TestMethod]
    [TestCategory(Category.ReflectionBinding)]
    public void NestedObject_ReflectionBinding_CascadesCorrectly()
    {
        // Arrange
        var saveObject = CreateNestedObjectTestData();
        
        // Act
        var result = ReflectionBinder.Bind<ParentModel>(saveObject);
        
        // Assert
        AssertNestedObjectBoundCorrectly(result!);
    }
    
    [TestMethod]
    [TestCategory(Category.SourceGeneratedBinding)]
    public void NestedObject_SourceGenBinding_CascadesCorrectly()
    {
        // Arrange
        var saveObject = CreateNestedObjectTestData();
        
        // Act
        var result = ParentModel.Bind(saveObject);
        
        // Assert
        AssertNestedObjectBoundCorrectly(result);
    }
    
    [TestMethod]
    [TestCategory(Category.SourceGeneratedBinding)]
    [TestCategory(Category.ReflectionBinding)]
    public void NestedObject_BothBindingApproaches_ProduceIdenticalResults()
    {
        // Arrange
        var saveObject = CreateNestedObjectTestData();
        
        // Act
        var reflectionResult = ReflectionBinder.Bind<ParentModel>(saveObject);
        var sourceGenResult = ParentModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(reflectionResult);
        Assert.IsNotNull(sourceGenResult);
        
        // Compare properties directly
        Assert.AreEqual(reflectionResult.Name, sourceGenResult.Name);
        Assert.AreEqual(reflectionResult.NestedObject!.Name, sourceGenResult.NestedObject!.Name);
        Assert.AreEqual(reflectionResult.NestedObject.Value, sourceGenResult.NestedObject.Value);
        Assert.AreEqual(reflectionResult.ItemArray!.Length, sourceGenResult.ItemArray!.Length);
        
        for (int i = 0; i < reflectionResult.ItemArray.Length; i++)
        {
            Assert.AreEqual(reflectionResult.ItemArray[i].Id, sourceGenResult.ItemArray[i].Id);
            Assert.AreEqual(reflectionResult.ItemArray[i].Description, sourceGenResult.ItemArray[i].Description);
        }
        
        // Serialize and compare JSON representations
        string reflectionJson = JsonSerializer.Serialize(reflectionResult);
        string sourceGenJson = JsonSerializer.Serialize(sourceGenResult);
        Assert.AreEqual(reflectionJson, sourceGenJson, "JSON representation should be identical");
    }
    
    #endregion
    
    #region Helper Methods
    
    private static SaveObject CreateSimpleTestObject()
    {
        return new SaveObject(
            [
                new("name", new Scalar<string>("name", "Test Empire")),
                new("capital", new Scalar<int>("capital", 5)),
                new("start_date", new Scalar<DateOnly>("start_date", new DateOnly(2200, 1, 1))),
                new("ironman", new Scalar<bool>("ironman", true)),
                new("achievement", new SaveArray([
                    new Scalar<int>("0", 1),
                    new Scalar<int>("1", 2),
                    new Scalar<int>("2", 3)
                ])),
                new("id", new Scalar<Guid>("id", new Guid("12345678-1234-5678-1234-567812345678"))),
                new("int_val", new Scalar<int>("int_val", 42)),
                new("string_val", new Scalar<string>("string_val", "hello")),
                new("date_value", new Scalar<DateOnly>("date_value", new DateOnly(2020, 01, 01))),
                new("array_value", new SaveArray([
                    new Scalar<int>("0", 1),
                    new Scalar<int>("1", 2),
                    new Scalar<int>("2", 3)
                ]))
            ]
        );
    }
    
    private static SaveObject CreateDictionaryTestObject()
    {
        return new SaveObject(
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
    }
    
    private static SaveObject CreateNestedObjectTestData()
    {
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
        
        return new SaveObject(
            [
                new("name", new Scalar<string>("name", "parent")),
                new("nested_object", nestedObj),
                new("item_array", arrayObjects)
            ]
        );
    }
    
    private static void AssertNestedObjectBoundCorrectly(ParentModel result)
    {
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
    
    #endregion
} 