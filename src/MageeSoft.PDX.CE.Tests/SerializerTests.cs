using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class SerializerTests
{
    private static string NormalizeLineEndings(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");

    [TestMethod]
    public void SimpleObject()
    {
        // Arrange
        var saveObject = new PdxObject([
                new("key", new PdxInt(42)),
            ]
        );

        // Act
        string result = saveObject.ToSaveString()!;

        // Assert
        Assert.AreEqual(
            expected: "{ key=42 }",
            actual: result
        );
    }

    [TestMethod]
    public void ComplexObject()
    {
        // Arrange
        var saveObject = new PdxObject([
                new("key1", new PdxInt(42)),
                new("key2", new PdxString("value")),
                new("key3", new PdxObject([
                            new("nestedKey1", new PdxInt(100)),
                            new("nestedKey2", new PdxString("nested"))
                        ]
                    )
                )
            ]
        );

        // Act
        string? result = saveObject.ToSaveString();

        // Assert
        Assert.AreEqual(
            expected: """
                      { key1=42 key2="value" key3={ nestedKey1=100 nestedKey2="nested" } }
                      """,
            actual: result
        );
    }

    [TestMethod]
    public void Scalars()
    {
        // Arrange
        var input = @"
string=hello
quoted=""hello world""
int=42
float=3.14
date=2200.01.01
bool=yes
empty_array={}
empty_object={}";

        // Act
        var element = PdxSaveReader.Read(input);
        var serialized = NormalizeLineEndings(element.ToSaveString()!);
        var reparsed = PdxSaveReader.Read(serialized);

        // Assert
        Assert.AreEqual(element.ToString(), reparsed.ToString(),
            "Simple scalar values should be preserved through serialization"
        );
    }

    [TestMethod]
    public void NestedObjects()
    {
        // Arrange
        var input = @"
root={
    array={
        value1
        value2
        {
            nested=true
        }
    }
    object={
        key1=value1
        key2={
            nested=yes
        }
    }
}";

        // Act
        var element = PdxSaveReader.Read(input);
        var serialized = NormalizeLineEndings(element.ToSaveString()!);
        var reparsed = PdxSaveReader.Read(serialized);

        // Assert
        Assert.AreEqual(
            element.ToString(),
            reparsed.ToString(),
            "Nested structures should be preserved through serialization"
        );
    }

    [TestMethod]
    public void SpecialCharacters()
    {
        // Arrange
        var input = @"
special={
    quoted=""value with spaces""
    escaped=""value with ""quotes"" inside""
    symbols=""value with {braces} and =equals""
    unquoted=simple_value
}";

        // Act
        var element = PdxSaveReader.Read(input);
        var serialized = NormalizeLineEndings(element.ToSaveString()!);
        var reparsed = PdxSaveReader.Read(serialized);

        // Assert
        Assert.AreEqual(
            expected: element.ToString(),
            actual: reparsed.ToString(),
            message: "Special characters should be preserved through serialization"
        );
    }

    [TestMethod]
    public void Empty()
    {
        // Arrange
        var input = "";

        // Act
        var root = PdxSaveReader.Read(input);

        // Assert
        Assert.AreEqual(
            expected: 0,
            actual: root.Properties.Length,
            message: "Empty input should produce an empty root element"
        );
    }

    [TestMethod]
    public void WhiteSpace()
    {
        // Arrange
        var input = "   \n   \t   ";
        var root = PdxSaveReader.Read(input);

        // Act & Assert
        Assert.AreEqual(
            expected: 0,
            actual: root.Properties.Length,
            message: "Whitespace-only input should produce an empty root element"
        );
    }

    [TestMethod]
    [Ignore(message: "Basically hangs/doesn't complete with a real file.")]
    public void Serialise_CanUpdateResource_ReturnsCorrectString()
    {
        // var root = PdxSaveReader.Read(File.ReadAllText("Stellaris/TestData/gamestate"));
        //
        // if (!root.TryGetSaveObject("country", out var countries)) Assert.Fail();
        //
        // if (!countries.TryGetSaveObject("0", out var country)) Assert.Fail();
        //
        // country.TryGetSaveObject("modules", out var modules);
        //
        // if (!modules.TryGetSaveObject("standard_economy_module", out var economyModule)) Assert.Fail();
        //
        // if (!economyModule.TryGetSaveObject("resources", out var resources)) Assert.Fail();
        //
        // foreach (var property in resources.Properties)
        // {
        //     if (property.Key == "energy")
        //     {
        //         if (property.Value is Scalar<int> energy)
        //         {
        //             energy.Value = 10000000;
        //             energy.RawText = "10000000";
        //
        //             break;
        //         }
        //     }
        // }
        //
        // var output = root.ToSaveString();
        // Assert.IsTrue(output.Contains("energy=10000000"), "Serialized output should contain updated energy value");
    }
}