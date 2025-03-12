using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class SaveTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void FromFile_WithValidSaveFile_ReturnsStellarisSave()
    {
        // Arrange
        var metaDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/meta"));
        var gameStateDocument = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate"));
        var documents = new GameSaveDocuments(metaDocument, gameStateDocument);

        // Act
        var save = new StellarisSave(documents);

        // Debug output
        TestContext.WriteLine($"Version: '{save.Version}'");
        TestContext.WriteLine($"Date: '{save.Date}'");
        TestContext.WriteLine($"Player: '{save.Player}'");
        TestContext.WriteLine($"GalaxyName: '{save.GalaxyName}'");

        // Assert
        Assert.IsNotNull(save);
        Assert.IsFalse(string.IsNullOrEmpty(save.Version));
        Assert.IsFalse(string.IsNullOrEmpty(save.Date));
        Assert.IsFalse(string.IsNullOrEmpty(save.GalaxyName));
    }

    [TestMethod]
    public void FromFile_WithInvalidExtension_ThrowsArgumentException()
    {
        // Arrange
        var saveFile = Path.Combine("Stellaris", "TestData", "ironman.txt");

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => StellarisSave.FromSave(saveFile));
    }

    [TestMethod]
    public void FromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var saveFile = Path.Combine("Stellaris", "TestData", "nonexistent.sav");

        // Act & Assert
        Assert.ThrowsException<FileNotFoundException>(() => StellarisSave.FromSave(saveFile));
    }
}