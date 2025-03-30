namespace MageeSoft.PDX.CE.Tests.Shared;

[TestClass]
public class SerializerTests
{
    private static string NormalizeLineEndings(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");

    private static string ReadTestFile(string filename) => File.ReadAllText(Path.Combine("Stellaris", "TestData", filename));
       
    private void AssertSerializationRoundTrip(string filename)
    {
        try
        {
            // Arrange
            var originalContent = NormalizeLineEndings(ReadTestFile(filename));
            //var parser = new Core.Parser(originalContent);
            
            // Act
            SaveObject element = Parser.Parse(originalContent);
            string serialized = NormalizeLineEndings(element.ToSaveString());
            
            //parser = new Core.Parser(serialized);
            SaveObject reparsed = Parser.Parse(serialized);
            string reserialized = NormalizeLineEndings(reparsed.ToSaveString());

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
    public void Serialise_SimpleObject_ReturnsCorrectString()
    {
        // Arrange
        var saveObject = new SaveObject([
                new("key", new Scalar<int>("42", 42)),
            ]
        );

        // Act
        string result = saveObject.ToSaveString();

        // Assert
        Assert.AreEqual(expected: "{ key=42 }", actual: result.RemoveFormatting());
    }

    [TestMethod]
    public void Serialise_ComplexObject_ReturnsCorrectString()
    {
        // Arrange
        var saveObject = new SaveObject([
                new("key1", new Scalar<int>("42", 42)),
                new("key2", new Scalar<string>("value", "value")),
                new("key3", new SaveObject([
                            new("nestedKey1", new Scalar<int>("100", 100)),
                            new("nestedKey2", new Scalar<string>("nested", "nested"))
                        ]
                    )
                )
            ]
        );

        // Act
        string result = saveObject.ToSaveString();

        // Assert
        Assert.AreEqual(expected: """{ key1=42 key2="value" key3={ nestedKey1=100 nestedKey2="nested" } }""", actual: result.RemoveFormatting());
    }
 
    [TestMethod]
    public void Serialize_Achievement_RoundTrip()
    {
        AssertSerializationRoundTrip("achievement.so");
    }

    [TestMethod]    
    public void Serialize_GameState_RoundTrip()
    {
        // This shows a serious problem with the parser
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
        var element = Parser.Parse(input);
        var serialized = NormalizeLineEndings(element.ToSaveString());
        var reparsed = Parser.Parse(serialized);

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
        var element = Parser.Parse(input);
        var serialized = NormalizeLineEndings(element.ToSaveString());
        var reparsed = Parser.Parse(serialized);

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
        var element = Parser.Parse(input);
        var serialized = NormalizeLineEndings(element.ToSaveString());
        var reparsed = Parser.Parse(serialized);

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
        Assert.ThrowsException<ArgumentException>(() => Parser.Parse(input), "Empty input should throw an ArgumentException");
    }

    [TestMethod]
    public void Serialize_WhitespaceOnlyInput()
    {
        // Arrange
        var input = "   \n   \t   ";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => Parser.Parse(input),
            "Whitespace-only input should throw an ArgumentException");
    }

    [TestMethod]
    public void Serialise_CanUpdateResource_ReturnsCorrectString()
    {
        var root = Parser.Parse(File.ReadAllText("Stellaris/TestData/gamestate"));

        if (!root.TryGetSaveObject("country", out var countries)) Assert.Fail();

        if (!countries.TryGetSaveObject("0", out var country)) Assert.Fail();

        country.TryGetSaveObject("modules", out var modules);

        if (!modules.TryGetSaveObject("standard_economy_module", out var economyModule)) Assert.Fail();

        if (!economyModule.TryGetSaveObject("resources", out var resources)) Assert.Fail();

        foreach (var property in resources.Properties)
        {
            if (property.Key == "energy")
            {
                if (property.Value is Scalar<int> energy)
                {
                    energy.Value = 10000000;
                    energy.RawText = "10000000";

                    break;
                }
            }
        }

        var output = root.ToSaveString();
        Assert.IsTrue(output.Contains("energy=10000000"), "Serialized output should contain updated energy value");
    }
} 