using System.Text.Json;
using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Test.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

[TestClass]
public class CompareBindingTests : BindingTests
{
    [TestMethod]
    public void NestedObject_ProduceIdenticalResults()
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
    
    [TestMethod]
    public void Dictionary_ProduceIdenticalResults()
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
}