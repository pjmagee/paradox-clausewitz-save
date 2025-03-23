using MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Stellaris.Common;

[TestClass]
public class SaveTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void FromFile_WithValidSaveFile_ReturnsStellarisSave()
    {
        // Act  
        StellarisSave save = StellarisSave.FromSave("Stellaris/TestData/ironman.sav");
        
        // Assert
        Assert.IsNotNull(save);
        Assert.IsNotNull(save.GameState);
        Assert.IsNotNull(save.Meta);
        
        Assert.AreEqual(expected: "Circinus v3.14.15926", actual: save.Meta.Version);
        Assert.AreEqual(new DateOnly(2250, 11, 15), save.Meta.Date);
        Assert.AreEqual("United Nations of Earth", save.Meta.Name);
        Assert.AreEqual(25, save.Meta.RequiredDlcs.Count);
    }

    [TestMethod]
    public void FromFile_WithInvalidExtension_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => StellarisSave.FromSave("Stellaris/TestData/ironman.txt"));
    }

    [TestMethod]
    public void FromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.ThrowsException<FileNotFoundException>(() => StellarisSave.FromSave("Stellaris/TestData/does_not_exist.sav"));
    }
}