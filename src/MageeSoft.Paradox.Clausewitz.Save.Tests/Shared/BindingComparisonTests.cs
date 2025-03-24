using System.Text.Json;
using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Test.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

/// <summary>
/// Tests that compare the reflection-based and source-generated binding approaches.
/// </summary>
[TestClass]
public class BindingComparisonTests
{
    [TestMethod]
    public void ComplexModel_ReflectionVsSourceGenerated_ProduceIdenticalResults()
    {
        // Arrange - create a complex object graph
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

        // Act - bind using both methods
        var reflectionResult = ReflectionBinder.Bind<ParentModel>(parentObj);
        var sourceGenResult = ParentModel.Bind(parentObj);

        // Assert - results should be identical
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
        
        // Alternative approach: Serialize both to JSON and compare the JSON strings
        string reflectionJson = JsonSerializer.Serialize(reflectionResult);
        string sourceGenJson = JsonSerializer.Serialize(sourceGenResult);
        Assert.AreEqual(reflectionJson, sourceGenJson, "JSON representation should be identical");
        
    }
    
    [TestMethod]
    public void Dictionary_ReflectionVsSourceGenerated_ProduceIdenticalResults()
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
        var reflectionResult = ReflectionBinder.Bind<TestSaveModel>(obj);
        var sourceGenResult = TestSaveModel.Bind(obj);

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
} 