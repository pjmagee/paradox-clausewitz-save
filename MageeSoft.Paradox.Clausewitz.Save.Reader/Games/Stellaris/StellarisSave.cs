using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;

namespace MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;

/// <summary>
/// Provides a high-level API for accessing Stellaris save data.
/// </summary>
public class StellarisSave
{
    public Meta Meta { get; private set; }
    public GameState GameState { get; private set; }
  
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
            using( var stream = File.OpenRead(saveFile))
            {
                using( var zip = new GameSaveZip(stream))
                {
                    var documents = zip.GetDocuments();

                    Meta? meta = null;
                    GameState ? gameState = null;
                    
                    try
                    {
                        meta = ReflectionBinder.Bind<Meta>(documents.MetaDocument.Root);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidDataException("Invalid meta file format", ex);
                    }
                    
                    try
                    {
                        gameState =  ReflectionBinder.Bind<GameState>(documents.GameStateDocument.Root);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidDataException("Invalid game state file format", ex);
                    }

                    return new StellarisSave { Meta = meta, GameState = gameState };
                }
            }            
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Invalid save file format", ex);
        }
    }
}