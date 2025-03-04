using System;
using System.IO;
using System.IO.Compression;

/// <summary>
/// Represents a game save zip file. (.sav)
/// </summary>          
public class GameSaveZip
{
    private const string ValidExtension = ".sav";
    private const string GameStateFileName = "gamestate";
    private const string MetaFileName = "meta";

    /// <summary>
    /// The documents contained in the game save zip file.
    /// </summary>
    public record GameSaveDocuments(GameStateDocument GameState, GameStateDocument Meta);

    /// <summary>
    /// Unzips a game save zip file.
    /// </summary>
    /// <param name="saveFile">The game save zip file.</param>
    /// <returns>The documents contained in the game save zip file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the save file does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when the file extension is not .sav.</exception>
    /// <exception cref="InvalidDataException">Thrown when the save file is corrupted or missing required files.</exception>
    /// <exception cref="IOException">Thrown when there is an error reading the save file.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the save file is denied.</exception>
    public static GameSaveDocuments Unzip(FileInfo saveFile)
    {
        if (saveFile == null)
            throw new ArgumentNullException(nameof(saveFile));

        if (!saveFile.Exists)
            throw new FileNotFoundException("Stellaris save file not found", saveFile.FullName);

        if (!string.Equals(saveFile.Extension, ValidExtension, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"File must have {ValidExtension} extension", nameof(saveFile));

        try
        {
            using var archive = ZipFile.OpenRead(saveFile.FullName);
            
            var gameStateEntry = archive.GetEntry(GameStateFileName) 
                ?? throw new InvalidDataException($"Save file does not contain a {GameStateFileName} file");
            
            var metaEntry = archive.GetEntry(MetaFileName)
                ?? throw new InvalidDataException($"Save file does not contain a {MetaFileName} file");

            string gameStateContent;
            string metaContent;

            try
            {
                using (var gameStateStream = gameStateEntry.Open())
                using (var gameStateReader = new StreamReader(gameStateStream))
                {
                    gameStateContent = gameStateReader.ReadToEnd();
                }

                using (var metaStream = metaEntry.Open())
                using (var metaReader = new StreamReader(metaStream))
                {
                    metaContent = metaReader.ReadToEnd();
                }
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidDataException("Failed to read save file contents. The file may be corrupted.", ex);
            }

            try
            {
                return new GameSaveDocuments(
                    GameState: GameStateDocument.Parse(gameStateContent),
                    Meta: GameStateDocument.Parse(metaContent)
                );
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is ArgumentException)
            {
                throw new InvalidDataException("Failed to parse save file contents. The file may be corrupted.", ex);
            }
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Failed to process save file. The file may be corrupted or in an unsupported format.", ex);
        }
    }
} 