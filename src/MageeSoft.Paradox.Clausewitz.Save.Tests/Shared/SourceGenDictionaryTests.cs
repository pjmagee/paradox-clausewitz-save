using MageeSoft.Paradox.Clausewitz.Save.Test.Models;
namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Shared;

[TestClass]
public class SourceGenDictionaryTests
{
    [TestMethod]
    public void BindIndexedDictionary_WithIntKeys_Works()
    {
        // Arrange - Create input with 0=, 1=, 2= format like in Stellaris saves
        var input = """
            scores={
                0=42.5
                1=37.8
                2=99.1
            }
            """;
            
        var parser = new Parser.Parser(input);
        var saveObject = parser.Parse();
        
        // Act
        var model = ImmutableDictionaryModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Scores);
        Assert.AreEqual(3, model.Scores.Count);
        Assert.AreEqual(42.5f, model.Scores[0]);
        Assert.AreEqual(37.8f, model.Scores[1]);
        Assert.AreEqual(99.1f, model.Scores[2]);
    }

    [TestMethod]
    public void BindIndexedDictionary_WithComplexValues_Works()
    {
        // Arrange - Create input with complex object values
        var input = """
            resources={
                0={
                    value=10
                    name="Test Resource"
                }
                1={
                    value=20
                    name="Another Resource"
                }
            }
            """;

        var parser = new Parser.Parser(input);
        
        var saveObject = parser.Parse();
        
        // Act
        var model = ImmutableDictionaryModel.Bind(saveObject);
        
        // Assert
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Resources);
        Assert.AreEqual(2, model.Resources.Count);
        Assert.AreEqual(10, model.Resources[0]?.Value);
        Assert.AreEqual("Test Resource", model.Resources[0]?.Name);
        Assert.AreEqual(20, model.Resources[1]?.Value);
        Assert.AreEqual("Another Resource", model.Resources[1]?.Name);
    }
} 