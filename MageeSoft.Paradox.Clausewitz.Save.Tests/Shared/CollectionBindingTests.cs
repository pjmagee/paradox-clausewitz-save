using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Test.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

/// <summary>
/// Tests specialized scenarios for collection binding (arrays, lists, immutable collections)
/// </summary>
[TestClass]
public class CollectionBindingTests
{
    #region Array Tests
    
    [TestMethod]
    public void Array_ReflectionBinding_BindsCorrectly()
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
    public void Array_SourceGenBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateArrayTestObject();
        
        // Act
        var result = TestModelForSourceGen.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ArrayValue);
        Assert.AreEqual(3, result.ArrayValue.Length);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ArrayValue);
    }
    
    #endregion
    
    #region List Tests
    
    [TestMethod]
    public void List_ReflectionBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateListTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<ImmutableListModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Values);
        Assert.AreEqual(3, result.Values.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.Values.ToArray());
    }
    
    [TestMethod]
    public void List_SourceGenBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateListTestObject();
        
        // Act
        var result = ImmutableListModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Values);
        Assert.AreEqual(3, result.Values.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.Values.ToArray());
    }
    
    #endregion
    
    #region Dictionary Tests
    
    [TestMethod]
    public void ImmutableDictionary_ReflectionBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateImmutableDictionaryTestObject();
        
        // Act
        var result = ReflectionBinder.Bind<ImmutableDictionaryModel>(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Resources);
        Assert.AreEqual(2, result.Resources.Count);
        
        Assert.IsNotNull(result.Resources[1]);
        Assert.AreEqual("First Item", result.Resources[1].Name);
        Assert.AreEqual(100, result.Resources[1].Value);
        
        Assert.IsNotNull(result.Resources[2]);
        Assert.AreEqual("Second Item", result.Resources[2].Name);
        Assert.AreEqual(200, result.Resources[2].Value);
        
        // Check scores
        Assert.IsNotNull(result.Scores);
        Assert.AreEqual(2, result.Scores.Count);
        Assert.AreEqual(42.5f, result.Scores[1]);
        Assert.AreEqual(99.9f, result.Scores[2]);
    }
    
    [TestMethod]
    public void ImmutableDictionary_SourceGenBinding_BindsCorrectly()
    {
        // Arrange
        var saveObject = CreateImmutableDictionaryTestObject();
        
        // Act
        var result = ImmutableDictionaryModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Resources);
        Assert.AreEqual(2, result.Resources.Count);
        
        Assert.IsNotNull(result.Resources[1]);
        Assert.AreEqual("First Item", result.Resources[1].Name);
        Assert.AreEqual(100, result.Resources[1].Value);
        
        Assert.IsNotNull(result.Resources[2]);
        Assert.AreEqual("Second Item", result.Resources[2].Name);
        Assert.AreEqual(200, result.Resources[2].Value);
        
        // Check scores
        Assert.IsNotNull(result.Scores);
        Assert.AreEqual(2, result.Scores.Count);
        Assert.AreEqual(42.5f, result.Scores[1]);
        Assert.AreEqual(99.9f, result.Scores[2]);
    }
    
    #endregion
    
    #region Repeated Property Tests
    
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
        
        Assert.AreEqual("SECTION_1", result.Sections[0].Design);
        Assert.AreEqual("1", result.Sections[0].Slot);
        
        Assert.AreEqual("SECTION_2", result.Sections[1].Design);
        Assert.AreEqual("2", result.Sections[1].Slot);
        
        // The 3rd section is 'none', which is included by the ReflectionBinder
        
        Assert.AreEqual("SECTION_3", result.Sections[3].Design);
        Assert.AreEqual("3", result.Sections[3].Slot);
    }
    
    [TestMethod]
    public void RepeatedProperties_SourceGenBinding_CollectsIntoArray()
    {
        // Arrange
        var saveObject = CreateRepeatedPropertiesTestObject();
        
        // Act
        var result = RepeatedPropertyModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Sections);
        Assert.AreEqual(3, result.Sections.Count);
        
        Assert.AreEqual("SECTION_1", result.Sections[0].Design);
        Assert.AreEqual("1", result.Sections[0].Slot);
        
        Assert.AreEqual("SECTION_2", result.Sections[1].Design);
        Assert.AreEqual("2", result.Sections[1].Slot);
        
        Assert.AreEqual("SECTION_3", result.Sections[2].Design);
        Assert.AreEqual("3", result.Sections[2].Slot);
    }
    
    #endregion
    
    #region Helper Methods
    
    private static SaveObject CreateArrayTestObject()
    {
        return new SaveObject(
            [
                new("array_value", new SaveArray([
                    new Scalar<int>("0", 1),
                    new Scalar<int>("1", 2),
                    new Scalar<int>("2", 3)
                ]))
            ]
        );
    }
    
    private static SaveObject CreateListTestObject()
    {
        return new SaveObject(
            [
                new("values", new SaveArray([
                    new Scalar<int>("0", 1),
                    new Scalar<int>("1", 2),
                    new Scalar<int>("2", 3)
                ])),
                new("strings", new SaveArray([
                    new Scalar<string>("0", "one"),
                    new Scalar<string>("1", "two"),
                    new Scalar<string>("2", "three")
                ]))
            ]
        );
    }
    
    private static SaveObject CreateImmutableDictionaryTestObject()
    {
        return new SaveObject(
            [
                new("resources", new SaveObject([
                    new("1", new SaveObject([
                        new("name", new Scalar<string>("name", "First Item")),
                        new("value", new Scalar<int>("value", 100))
                    ])),
                    new("2", new SaveObject([
                        new("name", new Scalar<string>("name", "Second Item")),
                        new("value", new Scalar<int>("value", 200))
                    ]))
                ])),
                new("scores", new SaveObject([
                    new("1", new Scalar<float>("1", 42.5f)),
                    new("2", new Scalar<float>("2", 99.9f))
                ]))
            ]
        );
    }
    
    private static SaveObject CreateRepeatedPropertiesTestObject()
    {
        return new SaveObject(
            [
                new("section", new SaveObject([
                    new("design", new Scalar<string>("design", "SECTION_1")),
                    new("slot", new Scalar<string>("slot", "1"))
                ])),
                new("section", new SaveObject([
                    new("design", new Scalar<string>("design", "SECTION_2")),
                    new("slot", new Scalar<string>("slot", "2"))
                ])),
                // Include a null section that should be skipped
                new("section", new Scalar<string>("section", "none")),
                new("section", new SaveObject([
                    new("design", new Scalar<string>("design", "SECTION_3")),
                    new("slot", new Scalar<string>("slot", "3"))
                ]))
            ]
        );
    }
    
    #endregion
} 