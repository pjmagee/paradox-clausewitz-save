using System.IO.Compression;

namespace StellarisSaveParser.Tests;

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
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sav");
        var saveFile = new FileInfo(tempPath);

        using (var archive = ZipFile.Open(saveFile.FullName, ZipArchiveMode.Create))
        {
            // Add gamestate file
            var gameStateEntry = archive.CreateEntry("gamestate");
            using (var writer = new StreamWriter(gameStateEntry.Open()))
            {
                writer.Write(@"galaxy={
    name={
        key=""Test Galaxy""
    }
}");
            }

            // Add meta file
            var metaEntry = archive.CreateEntry("meta");
            using (var writer = new StreamWriter(metaEntry.Open()))
            {
                writer.Write(@"version=""3.10.0""
date=""2200.01.01""
player={
    name=""Test Empire""
}");
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
            var documents = GameSaveZip.Unzip(saveFile);

            // Assert
            Assert.IsNotNull(documents);
            Assert.IsNotNull(documents.GameState);
            Assert.IsNotNull(documents.Meta);

            // Verify gamestate content
            var galaxy = documents.GameState.RootElement
                .EnumerateObject()
                .First(p => p.Key == "galaxy");
            
            var galaxyName = galaxy.Value
                .EnumerateObject()
                .First(p => p.Key == "name")
                .Value;

            Assert.Contains("Test Galaxy", galaxyName.ToString());

            // Verify meta content
            var version = documents.Meta.RootElement
                .EnumerateObject()
                .First(p => p.Key == "version")
                .Value;

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
        Assert.Throws<FileNotFoundException>(() => GameSaveZip.Unzip(nonExistentFile));
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
            var ex = Assert.Throws<InvalidDataException>(() => GameSaveZip.Unzip(invalidFile));
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
        FileInfo? nullFile = null;
        Assert.Throws<ArgumentNullException>(() => GameSaveZip.Unzip(nullFile!));
    }

    [TestMethod]
    public void TestUnzipInvalidExtension()
    {
        // Arrange
        var invalidFile = new FileInfo(Path.GetTempFileName());

        try
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => GameSaveZip.Unzip(invalidFile));
            Assert.Contains(".sav extension", ex.Message);
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
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sav");
        var corruptedFile = new FileInfo(tempPath);
        File.WriteAllText(corruptedFile.FullName, "This is not a valid zip file");

        try
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidDataException>(() => GameSaveZip.Unzip(corruptedFile));
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
        var saveFile = new FileInfo("./TestData/ironman.sav");
        Assert.IsTrue(saveFile.Exists, "ironman.sav test file not found");

        // Act
        var documents = GameSaveZip.Unzip(saveFile);

        // Assert
        Assert.IsNotNull(documents);
        Assert.IsNotNull(documents.GameState);
        Assert.IsNotNull(documents.Meta);

        // Log some basic info about the save
        _context.WriteLine("Meta Information:");
        foreach (var prop in documents.Meta.RootElement.EnumerateObject())
        {
            _context.WriteLine($"{prop.Key}: {prop.Value}");
        }

        // Verify gamestate structure
        var gamestate = documents.GameState.RootElement;

        // Check for required top-level sections
        var topLevelKeys = gamestate.EnumerateObject().Select(p => p.Key).ToList();
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
        var galaxy = gamestate.EnumerateObject().First(p => p.Key == "galaxy").Value;
        Assert.IsNotNull(galaxy);

        // Verify player info
        var player = gamestate.EnumerateObject().First(p => p.Key == "player").Value;
        Assert.IsNotNull(player);

        // Output some interesting save info
        var date = documents.Meta.RootElement.EnumerateObject()
            .FirstOrDefault(p => p.Key == "date");
        
        if (date.Key != null)
            _context.WriteLine($"\nSave Date: {date.Value}");

        var version = documents.Meta.RootElement.EnumerateObject()
            .FirstOrDefault(p => p.Key == "version");
        
        if (version.Key != null)
            _context.WriteLine($"Game Version: {version.Value}");
    }
} 