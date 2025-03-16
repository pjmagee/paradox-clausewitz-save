using System.Text.Json.Nodes;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris.Save;

[TestClass]
public class SaveTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void FromFile_WithValidSaveFile_ReturnsStellarisSave()
    {
        // Act  
        StellarisSave save = StellarisSave.FromSave("Stellaris/TestData/ironman.sav");
        
        TestContext.Write(System.Text.Json.JsonSerializer.Serialize(save));
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