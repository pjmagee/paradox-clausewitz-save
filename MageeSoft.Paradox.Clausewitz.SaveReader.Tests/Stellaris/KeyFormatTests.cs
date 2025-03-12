using Microsoft.VisualStudio.TestTools.UnitTesting;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class KeyFormatTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Parse_SimpleProperty_KeyDoesNotIncludeEqualsSign()
    {
        // Arrange
        string input = "test=value";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var testProp = root.Properties[0];
        
        // Check that the key does not include the equals sign
        TestContext.WriteLine($"Key: '{testProp.Key}'");
        Assert.AreEqual("test", testProp.Key, "Property key should be 'test' without equals sign");
    }

    [TestMethod]
    public void Parse_SimpleArray_KeyDoesNotIncludeEqualsSign()
    {
        // Arrange
        string input = "test={ 1 2 3 }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var testProp = root.Properties[0];
        
        // Check that the key does not include the equals sign
        TestContext.WriteLine($"Key: '{testProp.Key}'");
        Assert.AreEqual("test", testProp.Key, "Property key should be 'test' without equals sign");
    }

    [TestMethod]
    public void Parse_NestedObject_KeysDoNotIncludeEqualsSign()
    {
        // Arrange
        string input = @"
        country={
            name=""Test Empire""
            capital=5
        }";
        var parser = new Parser.Parser(input);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.IsInstanceOfType(result, typeof(SaveObject));
        var root = (SaveObject)result;
        
        Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
        var countryProp = root.Properties[0];
        
        // Check that the key does not include the equals sign
        TestContext.WriteLine($"Root key: '{countryProp.Key}'");
        Assert.AreEqual("country", countryProp.Key, "Property key should be 'country' without equals sign");
        
        Assert.IsInstanceOfType(countryProp.Value, typeof(SaveObject));
        var country = (SaveObject)countryProp.Value;
        
        // Check nested properties
        Assert.AreEqual(2, country.Properties.Length, "Country should have 2 properties");
        
        foreach (var prop in country.Properties)
        {
            TestContext.WriteLine($"Nested key: '{prop.Key}'");
            Assert.IsFalse(prop.Key.EndsWith("="), $"Property key '{prop.Key}' should not end with '='");
        }
        
        // Check specific nested properties
        var nameProp = country.Properties.First(p => p.Key == "name");
        Assert.IsNotNull(nameProp, "Should have a property with key 'name'");
        
        var capitalProp = country.Properties.First(p => p.Key == "capital");
        Assert.IsNotNull(capitalProp, "Should have a property with key 'capital'");
    }
} 