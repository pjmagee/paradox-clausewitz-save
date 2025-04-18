using MageeSoft.PDX.CE.Save;

namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class StellarisSaveTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void FromSave_WithValidSaveFile_ReturnsStellarisSave()
    {
        // Act  
        StellarisSave save = StellarisSave.FromSave(Path.Combine("Stellaris", "TestData", "ironman.sav"));
        
        // Assert
        Assert.IsNotNull(save);
        Assert.IsNotNull(save.GameState);
        Assert.IsNotNull(save.Meta);
        
        Assert.AreEqual(expected: "Circinus v3.14.15926", actual: save.Meta.FindProperty("version").Value.Value<string>());
        Assert.AreEqual(new DateOnly(2250, 11, 15), save.Meta.FindProperty("date").Value.Value<DateOnly>());
        Assert.AreEqual("United Nations of Earth", save.Meta.FindProperty("name").Value.Value<string>());
        Assert.IsTrue(save.Meta.FindProperty("ironman").Value.Value<bool>());
    }

    [TestMethod]
    public void FromSave_WithInvalidExtension_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(
            action: () => StellarisSave.FromSave(Path.Combine("Stellaris", "TestData", "ironman.txt")),
            message: "The file 'TestData/ironman.txt' is not a valid Stellaris save file. Expected extension '.sav'."
        );
    }

    [TestMethod]
    public void FromSave_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.ThrowsException<FileNotFoundException>(
            action: () => StellarisSave.FromSave(Path.Combine("Stellaris", "TestData", "does_not_exist.sav")),
            message: "The file 'TestData/does_not_exist.sav' does not exist."
        );
    }
}