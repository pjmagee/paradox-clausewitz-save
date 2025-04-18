using System.IO.Compression;
using MageeSoft.PDX.CE.Save.Games.Stellaris;

namespace MageeSoft.PDX.CE.Tests;

[TestClass]
public class GameSaveZipTests
{
    public TestContext TestContext { get; set; } = null!;

    private static FileInfo CreateTestSaveFile()
    {
        var saveFile = new FileInfo(Guid.NewGuid() + ".sav");

        using (var archive = ZipFile.Open(saveFile.FullName, ZipArchiveMode.Create))
        {
            // Add gamestate file
            var gameStateEntry = archive.CreateEntry("gamestate");
            using (var writer = new StreamWriter(gameStateEntry.Open()))
            {
                writer.Write("""
                             galaxy={ 
                               name={ 
                                 key="Test Galaxy"
                               }
                             }
                             """);
            }

            // Add meta file
            var metaEntry = archive.CreateEntry("meta");
            using (var writer = new StreamWriter(metaEntry.Open()))
            {
                writer.Write("""
                             version="3.10.0"
                             date="2200.01.01"
                             """);
            }
        }

        return saveFile;
    }

    [TestMethod]
    public void TestUnzipValidSaveFile()
    {
        // Arrange
        var saveFile = CreateTestSaveFile();

        try
        {
            // Act
            using (var gameSaveZip = GameSaveZip.Open(saveFile))
            {
                var documents = gameSaveZip.GetDocuments();

                // Assert
                Assert.IsNotNull(documents);
                Assert.IsNotNull(documents.GameStateDocument);
                Assert.IsNotNull(documents.MetaDocument);

                // Verify gamestate content
                var gameState = documents.GameStateDocument.Root;
                Assert.IsNotNull(gameState);
            
                gameState.TryGet<PdxObject>("galaxy", out var galaxyObj);
                Assert.IsNotNull(galaxyObj);
            
                galaxyObj.TryGet<PdxObject>("name", out var name);
                Assert.IsNotNull(name);
            
                name.TryGetString("key", out var galaxyName);
                Assert.AreEqual("Test Galaxy", galaxyName);
            
                // Verify meta content
                var meta = documents.MetaDocument.Root;
                Assert.IsNotNull(meta);
                meta.TryGetString("version", out var version);
                Assert.IsNotNull(version);
                Assert.Contains("3.10.0", version);
            }
        }
        finally
        {
            // Cleanup
            if (saveFile.Exists)
                saveFile.Delete();
        }
    }

    [TestMethod]
    public void TestUnzipNonExistentFile()
    {
        // Arrange
        var nonExistentFile = new FileInfo("nonexistent.sav");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => GameSaveZip.Open(nonExistentFile));
    }

    [TestMethod]
    public void TestUnzipInvalidZipFile()
    {
        // Arrange
        var invalidFile = new FileInfo(Guid.NewGuid() + ".sav");
        File.WriteAllText(invalidFile.FullName, "Not a zip file");

        try
        {
            // Act & Assert
            Assert.Throws<InvalidDataException>(() =>
            {
                using (var zip = GameSaveZip.Open(invalidFile))
                {
                    zip.GetDocuments();
                }
            });
        }
        finally
        {
            // Cleanup
            if (invalidFile.Exists)
                invalidFile.Delete();
        }
    }

    [TestMethod]
    public void TestUnzipCorruptedZip()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sav");
        var corruptedFile = new FileInfo(tempPath);
        File.WriteAllText(corruptedFile.FullName, "This is not a valid zip file");

        try
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidDataException>(() => GameSaveZip.Open(corruptedFile));
            Assert.Contains("End of Central Directory record could not be found", ex.Message);
        }
        finally
        {
            // Cleanup
            if (corruptedFile.Exists)
                corruptedFile.Delete();
        }
    }

    [TestMethod]
    public void TestUnzipIronmanSave()
    {
        // Arrange
        var saveFile = new FileInfo(Path.Combine("Stellaris", "TestData", "ironman.sav"));
        Assert.IsTrue(saveFile.Exists, "ironman.sav test file not found");

        // Act
        using var gameSaveZip = GameSaveZip.Open(saveFile);
        var documents = gameSaveZip.GetDocuments();

        // Assert
        Assert.IsNotNull(documents);
        Assert.IsNotNull(documents.GameStateDocument);
        Assert.IsNotNull(documents.MetaDocument);

        // Log some basic info about the save
        TestContext.WriteLine("Meta Information:");
        var meta = documents.MetaDocument.Root;
        Assert.IsNotNull(meta);
        foreach (var prop in meta.Properties)
        {
            TestContext.WriteLine($"{prop.Key}: {prop.Value}");
        }

        // Verify gamestate structure
        var gamestate = documents.GameStateDocument.Root;
        Assert.IsNotNull(gamestate);

        // Check for required top-level sections
        var topLevelKeys = gamestate.Properties.Select(p => p.Key).ToList();
        TestContext.WriteLine("\nTop-level gamestate sections:");
        foreach (var key in topLevelKeys)
        {
            TestContext.WriteLine(key.Value<string>());
        }

        // Verify essential sections exist
        Assert.Contains(topLevelKeys, k => k.Value<string>() == "galaxy", "Missing galaxy section");
        Assert.Contains(topLevelKeys, k => k.Value<string>() == "player", "Missing player section");
        Assert.Contains(topLevelKeys, k => k.Value<string>() == "country", "Missing country section");

        // Verify galaxy structure
        var galaxy = gamestate.Properties.First(p => p.Key.Value<string>() == "galaxy").Value;
        Assert.IsNotNull(galaxy);

        // Verify player info
        var player = gamestate.Properties.First(p => p.Key.Value<string>() == "player").Value;
        Assert.IsNotNull(player);

        // Output some interesting save info
        var date = meta.Properties.FirstOrDefault(p => p.Key.Value<string>() == "date");
        
        if (date.Key != null)
            TestContext.WriteLine($"\nSave Date: {date.Value}");

        var version = meta.Properties.FirstOrDefault(p => p.Key.Value<string>() == "version");
        
        if (version.Key != null)
            TestContext.WriteLine($"Game Version: {version.Value}");
    }
} 