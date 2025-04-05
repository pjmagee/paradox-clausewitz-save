namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class ModelSerializerTests
{
    private static string ReadTestFile(string filename) => File.ReadAllText(Path.Combine("Stellaris", "TestData", filename));
    
    private static string NormalizeLineEndings(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");
    
    private void AssertSerializationRoundTrip(string filename)
    {
        try
        {
            // Arrange
            var originalContent = NormalizeLineEndings(ReadTestFile(filename));
            
            // Act
            SaveObject element = Parser.Parse(originalContent);
            string serialized = NormalizeLineEndings(element.ToSaveString());
            
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
    public void Serialize_GameState_RoundTrip()
    {
        AssertSerializationRoundTrip("gamestate");
    }

    [TestMethod]
    public void Serialize_Meta_RoundTrip()
    {
        AssertSerializationRoundTrip("meta");
    }
}