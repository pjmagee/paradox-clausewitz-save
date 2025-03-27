using System.Text.Json;
using MageeSoft.Paradox.Clausewitz.Save.Parser;
using MageeSoft.Paradox.Clausewitz.Save.Test.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

public class BindingTests
{
    protected static SaveObject CreateSimpleTestObject()
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
    
    protected static SaveObject CreateDictionaryTestObject()
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
    
    protected static SaveObject CreateNestedObjectTestData()
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
   
    protected static SaveObject CreateArrayTestObject()
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
    
    protected static SaveObject CreateListTestObject()
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
    
    protected static SaveObject CreateRepeatedPropertiesTestObject()
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
    
    protected static void AssertNestedObjectBoundCorrectly(ParentModel result)
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
}