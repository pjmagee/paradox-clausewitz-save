using System.IO.Compression;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris.Zip;

[TestClass]
public class GameSaveZipTests
{
    private readonly TestContext _context;

    public GameSaveZipTests(TestContext context)
    {
        _context = context;
    }

    private static FileInfo CreateTestSaveFile()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sav");
        var saveFile = new FileInfo(tempPath);

        using (var archive = ZipFile.Open(saveFile.FullName, ZipArchiveMode.Create))
        {
            // Add gamestate file
            var gameStateEntry = archive.CreateEntry("gamestate");
            using (var writer = new StreamWriter(gameStateEntry.Open()))
            {
                writer.Write(@"galaxy={ name={ key=""Test Galaxy"" } }");
            }

            // Add meta file
            var metaEntry = archive.CreateEntry("meta");
            using (var writer = new StreamWriter(metaEntry.Open()))
            {
                writer.Write(@"version=""3.10.0"" date=""2200.01.01"" player={    name=""Test Empire"" }");
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
            using var stream = File.OpenRead(saveFile.FullName);
            using var gameSaveZip = new GameSaveZip(stream);
            var documents = gameSaveZip.GetDocuments();

            // Assert
            Assert.IsNotNull(documents);
            Assert.IsNotNull(documents.GameStateDocument);
            Assert.IsNotNull(documents.MetaDocument);

            // Verify gamestate content
            var gameState = documents.GameStateDocument.Root as SaveObject;
            Assert.IsNotNull(gameState);
            var galaxy = gameState.Properties.First(p => p.Key == "galaxy");
            
            var galaxyObj = galaxy.Value as SaveObject;
            Assert.IsNotNull(galaxyObj);
            var galaxyName = galaxyObj.Properties.First(p => p.Key == "name").Value;

            Assert.Contains("Test Galaxy", galaxyName.ToString());

            // Verify meta content
            var meta = documents.MetaDocument.Root as SaveObject;
            Assert.IsNotNull(meta);
            var version = meta.Properties.First(p => p.Key == "version").Value;

            Assert.Contains("3.10.0", version.ToString());
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
        Assert.Throws<FileNotFoundException>(() => File.OpenRead(nonExistentFile.FullName));
    }

    [TestMethod]
    public void TestUnzipInvalidZipFile()
    {
        // Arrange
        var invalidFile = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sav"));
        File.WriteAllText(invalidFile.FullName, "Not a zip file");

        try
        {
            // Act & Assert
            using var stream = File.OpenRead(invalidFile.FullName);
            var ex = Assert.Throws<InvalidDataException>(() => new GameSaveZip(stream));
            Assert.Contains("Central Directory corrupt", ex.Message);
        }
        finally
        {
            // Cleanup
            if (invalidFile.Exists)
                invalidFile.Delete();
        }
    }

    [TestMethod]
    public void TestUnzipNullFile()
    {
        // Act & Assert
        Stream? nullStream = null;
        Assert.Throws<ArgumentNullException>(() => new GameSaveZip(nullStream!));
    }

    [TestMethod]
    public void TestUnzipInvalidExtension()
    {
        // Arrange
        var invalidFile = new FileInfo(Path.GetTempFileName());

        try
        {
            // Act & Assert
            using var stream = File.OpenRead(invalidFile.FullName);
            var ex = Assert.Throws<InvalidDataException>(() => new GameSaveZip(stream));
            Assert.Contains("Central Directory corrupt", ex.Message);
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
            using var stream = File.OpenRead(corruptedFile.FullName);
            var ex = Assert.Throws<InvalidDataException>(() => new GameSaveZip(stream));
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
        using var stream = File.OpenRead(saveFile.FullName);
        using var gameSaveZip = new GameSaveZip(stream);
        var documents = gameSaveZip.GetDocuments();

        // Assert
        Assert.IsNotNull(documents);
        Assert.IsNotNull(documents.GameStateDocument);
        Assert.IsNotNull(documents.MetaDocument);

        // Log some basic info about the save
        _context.WriteLine("Meta Information:");
        var meta = documents.MetaDocument.Root as SaveObject;
        Assert.IsNotNull(meta);
        foreach (var prop in meta.Properties)
        {
            _context.WriteLine($"{prop.Key}: {prop.Value}");
        }

        // Verify gamestate structure
        var gamestate = documents.GameStateDocument.Root;
        Assert.IsNotNull(gamestate);

        // Check for required top-level sections
        var topLevelKeys = gamestate.Properties.Select(p => p.Key).ToList();
        _context.WriteLine("\nTop-level gamestate sections:");
        foreach (var key in topLevelKeys)
        {
            _context.WriteLine(key);
        }

        // Verify essential sections exist
        Assert.Contains(topLevelKeys, k => k == "galaxy", "Missing galaxy section");
        Assert.Contains(topLevelKeys, k => k == "player", "Missing player section");
        Assert.Contains(topLevelKeys, k => k == "country", "Missing country section");

        // Verify galaxy structure
        var galaxy = gamestate.Properties.First(p => p.Key == "galaxy").Value;
        Assert.IsNotNull(galaxy);

        // Verify player info
        var player = gamestate.Properties.First(p => p.Key == "player").Value;
        Assert.IsNotNull(player);

        // Output some interesting save info
        var date = meta.Properties.FirstOrDefault(p => p.Key == "date");
        
        if (date.Key != null)
            _context.WriteLine($"\nSave Date: {date.Value}");

        var version = meta.Properties.FirstOrDefault(p => p.Key == "version");
        
        if (version.Key != null)
            _context.WriteLine($"Game Version: {version.Value}");
    }
} 