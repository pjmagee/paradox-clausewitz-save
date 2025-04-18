using MageeSoft.PDX.CE.Save.Games.Stellaris;

namespace MageeSoft.PDX.CE.Save;

/// <summary>
/// Provides a high-level API for accessing Stellaris save data.
/// </summary>
public class StellarisSave
{
    /// <summary>
    /// The meta information of the save file.
    /// </summary>
    public PdxObject Meta { get; private set; } = null!;
    
    /// <summary>
    ///  The gamestate information of the save file.
    /// </summary>
    public PdxObject GameState { get; private set; } = null!;

    // Private constructor to prevent external instantiation.
    private StellarisSave()
    {
        
    }

    public static StellarisSave FromSave(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));

        if (!fileInfo.Exists)
            throw new FileNotFoundException("Stellaris save file not found", fileInfo.FullName);

        if (!fileInfo.Extension.Equals(".sav", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid save file format. Expected .sav file.", nameof(fileInfo));
        
        return FromSave(fileInfo.FullName);
    }
    
    public static StellarisSave FromSave(string saveFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveFile, nameof(saveFile));

        if (string.IsNullOrEmpty(saveFile))
            throw new ArgumentException("Save file path cannot be null or empty", nameof(saveFile));

        if (!File.Exists(saveFile))        
            throw new FileNotFoundException("Stellaris save file not found", saveFile);

        if (!Path.GetExtension(saveFile).Equals(".sav", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid save file format. Expected .sav file.", nameof(saveFile));
        
        try 
        {
            using ( var zip = GameSaveZip.Open(saveFile))
            {
                var documents = zip.GetDocuments();

                PdxObject? meta = null;
                PdxObject? gameState = null;
                    
                try
                {
                    meta = documents.MetaDocument.Root;
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Invalid meta file format", ex);
                }
                    
                try
                {
                    gameState = documents.GameStateDocument.Root;
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Invalid game state file format", ex);
                }

                return new StellarisSave { Meta = meta!, GameState = gameState! };
            }        
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Invalid save file format", ex);
        }
    }

    public void WriteTo(string path)
    {
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (var zip = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Create))
        {
            var metaEntry = zip.CreateEntry("meta");
            using (var writer = new StreamWriter(metaEntry.Open()))
                writer.Write(Meta.ToString());
            var gamestateEntry = zip.CreateEntry("gamestate");
            using (var writer = new StreamWriter(gamestateEntry.Open()))
                writer.Write(GameState.ToString());
        }
    }

    public void WriteTo(FileInfo file)
    {
        WriteTo(file.FullName);
    }
}