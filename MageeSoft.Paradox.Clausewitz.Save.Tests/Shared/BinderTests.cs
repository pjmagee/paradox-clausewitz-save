using System.Text.Json;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

[TestClass]
public class BinderTests
{
    [TestMethod]
    public void BindSimpleModel_HappyPath()
    {
        // Arrange
        var obj = new SaveObject(
            [
                new("int_value", new Scalar<int>("int_value", 42)),
                new("string_value", new Scalar<string>("string_value", "hello")),
                new("date_value", new Scalar<DateOnly>("date_value", new DateOnly(2020, 01, 01))),
                new("array_value", new SaveArray([ 
                        new Scalar<int>("0", 1), 
                        new Scalar<int>("1", 2), 
                        new Scalar<int>("2", 3)]
                ))
            ]
        );

        // Act
        // Note: Using ReflectionBinder until source-generated Bind is working
        var result = ReflectionBinder.Bind<SimpleTestModel>(obj);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.IntValue);
        Assert.AreEqual("hello", result.StringValue);
        Assert.AreEqual(new DateOnly(2020, 01, 01), result.DateValue);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ArrayValue);
    }
    
    [TestMethod]
    public void BindTestModel_WithDictionary()
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
        // Note: Using ReflectionBinder until source-generated Bind is working
        var result = ReflectionBinder.Bind<TestSaveModel>(obj);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.DictValue.Count);
        Assert.AreEqual("one", result.DictValue[1]);
        Assert.AreEqual("two", result.DictValue[2]);
        Assert.AreEqual("three", result.DictValue[3]);
    }
} 