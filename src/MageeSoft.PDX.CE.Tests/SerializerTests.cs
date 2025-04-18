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
                new(new PdxString("key"), new PdxInt(42)),
            ]
        );

        // Act
        string result = saveObject.ToString()!;

        // Assert
        Assert.AreEqual(
            expected: "{\r\n\tkey=42\r\n}",
            actual: result
        );
    }

    [TestMethod]
    public void ComplexObject()
    {
        // Arrange
        var saveObject = new PdxObject([
                new(new PdxString("key1"), new PdxInt(42)),
                new(new PdxString("key2"), new PdxString("value")),
                new(new PdxString("key3"), new PdxObject([
                            new(new PdxString("nestedKey1"), new PdxInt(100)),
                            new(new PdxString("nestedKey2"), new PdxString("nested", wasQuoted: true))
                        ]
                    )
                )
            ]
        );

        // Act
        string? result = saveObject.ToString();

        // Assert
        Assert.AreEqual(
            expected: """
                      { 
                        key1=42 
                        key2="value" 
                        key3={
                            nestedKey1=100
                            nestedKey2="nested"
                        }
                      }
                      """,
            actual: result
        );
    }

    [TestMethod]
    public void Scalars()
    {
        // Arrange
        var input = """
                    string=hello
                    quoted="hello world"
                    int=42
                    float=3.14
                    date="2200.01.01"
                    bool=yes
                    empty_array={}
                    empty_object={}
                    """;

        // Act
        var element = PdxSaveReader.Read(input);
        var serialized = NormalizeLineEndings(element.ToString()!);
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
        var input = """
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
                    }
                    """;

        // Act
        var element = PdxSaveReader.Read(input);
        var serialized = NormalizeLineEndings(element.ToString()!);
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
        var input = """
special={
    quoted="value with spaces"
    symbols="value with {braces} and =equals"
    unquoted=simple_value
}
""";

        // Act
        var element = PdxSaveReader.Read(input);
        var serialized = NormalizeLineEndings(element.ToString()!);
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
            actual: root.Properties.Count,
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
            actual: root.Properties.Count,
            message: "Whitespace-only input should produce an empty root element"
        );
    }
}