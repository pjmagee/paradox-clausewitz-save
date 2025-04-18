namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class TestDataTests
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
            var element = PdxSaveReader.Read(originalContent);
            string serialized = NormalizeLineEndings(element.ToString()!);
            
            var reparsed = PdxSaveReader.Read(serialized);
            string reserialized = NormalizeLineEndings(reparsed.ToString()!);

            try
            {
                // Then verify the serialized output is stable
                var serializedLines = serialized.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var reserializedLines = reserialized.Split('\n', StringSplitOptions.RemoveEmptyEntries);

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