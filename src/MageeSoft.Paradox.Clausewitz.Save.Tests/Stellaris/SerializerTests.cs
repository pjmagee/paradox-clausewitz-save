using MageeSoft.Paradox.Clausewitz.Save.Parser;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Stellaris;

[TestClass]
public class SerializerTests
{
    private static string NormalizeLineEndings(string text) => 
        text.Replace("\r\n", "\n").Replace("\r", "\n");

    private static string ReadTestFile(string filename) =>
        File.ReadAllText(Path.Combine("Stellaris", "TestData", filename));

    private void AssertSerializationRoundTrip(string filename)
    {
        try
        {
            // Arrange
            var originalContent = NormalizeLineEndings(ReadTestFile(filename));
            var parser = new Parser.Parser(originalContent);
            
            // Act
            var element = parser.Parse();
            var serialized = NormalizeLineEndings(element.ToSaveString());
            
            parser = new Parser.Parser(serialized);
            var reparsed = parser.Parse();
            var reserialized = NormalizeLineEndings(reparsed.ToSaveString());

            try 
            {
                // First verify the content is semantically equivalent after a round trip
                Assert.AreEqual(element.ToString(), reparsed.ToString(), 
                    $"Parsed objects should be semantically equivalent for file {filename}");
            }
            catch (AssertFailedException)
            {
                Console.WriteLine($"Original parsed structure:\n{element}");
                Console.WriteLine($"\nReparsed structure:\n{reparsed}");
                throw;
            }

            try
            {
                // Then verify the serialized output is stable
                var serializedLines = serialized.Split('\n');
                var reserializedLines = reserialized.Split('\n');

                Assert.AreEqual(serializedLines.Length, reserializedLines.Length,$"Serialized output should have the same number of lines as the original for file {filename}");


                for (int i = 0; i < serializedLines.Length; i++)
                {
                    Assert.AreEqual(serializedLines[i], reserializedLines[i], $"Serialized output should be equal across multiple serializations for file {filename}");
                }
            }
            catch (AssertFailedException)
            {
                Console.WriteLine($"First serialization:\n{serialized}");
                Console.WriteLine($"\nSecond serialization:\n{reserialized}");
                throw;
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to process {filename}: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    [TestMethod]
    public void Serialize_Achievement_RoundTrip()
    {
        AssertSerializationRoundTrip("achievement.so");
    }

    [TestMethod]
    public void Serialize_GameState_RoundTrip()
    {
        AssertSerializationRoundTrip("gamestate");
    }

    [TestMethod]
    public void Serialize_Army_RoundTrip()
    {
        AssertSerializationRoundTrip("army.so");
    }

    [TestMethod]
    public void Serialize_Meta_RoundTrip()
    {
        AssertSerializationRoundTrip("meta");
    }

    [TestMethod]
    public void Serialize_SimpleScalars()
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
        var parser = new Parser.Parser(input);
        var element = parser.Parse();
        var serialized = NormalizeLineEndings(element.ToSaveString());
        parser = new Parser.Parser(serialized);
        var reparsed = parser.Parse();

        // Assert
        Assert.AreEqual(element.ToString(), reparsed.ToString(), 
            "Simple scalar values should be preserved through serialization");
    }

    [TestMethod]
    public void Serialize_NestedStructures()
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
        var parser = new Parser.Parser(input);
        var element = parser.Parse();
        var serialized = NormalizeLineEndings(element.ToSaveString());
        parser = new Parser.Parser(serialized);
        var reparsed = parser.Parse();

        // Assert
        Assert.AreEqual(element.ToString(), reparsed.ToString(),
            "Nested structures should be preserved through serialization");
    }

    [TestMethod]
    public void Serialize_SpecialCharacters()
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
        var parser = new Parser.Parser(input);
        var element = parser.Parse();
        var serialized = NormalizeLineEndings(element.ToSaveString());
        parser = new Parser.Parser(serialized);
        var reparsed = parser.Parse();

        // Assert
        Assert.AreEqual(element.ToString(), reparsed.ToString(),
            "Special characters should be preserved through serialization");
    }

    [TestMethod]
    public void Serialize_EmptyInput()
    {
        // Arrange
        var input = "";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new Parser.Parser(input),
            "Empty input should throw an ArgumentException");
    }

    [TestMethod]
    public void Serialize_WhitespaceOnlyInput()
    {
        // Arrange
        var input = "   \n   \t   ";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new Parser.Parser(input),
            "Whitespace-only input should throw an ArgumentException");
    }
} 