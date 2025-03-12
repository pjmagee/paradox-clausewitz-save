using System.IO.Compression;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

/// <summary>
/// Represents a game save zip file. (.sav)
/// </summary>          
public class GameSaveZip : IDisposable
{
    const string ValidExtension = ".sav";
    const string GameStateFileName = "gamestate";
    const string MetaFileName = "meta";

    readonly ZipArchive _archive;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameSaveZip"/> class.
    /// </summary>
    /// <param name="stream">The stream containing the zip file.</param>
    public GameSaveZip(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        _archive = new ZipArchive(stream, ZipArchiveMode.Read);
    }

    /// <summary>
    /// Gets the documents contained in the save file.
    /// </summary>
    /// <returns>The game state and meta documents.</returns>
    public GameSaveDocuments GetDocuments()
    {
        var gameState = _archive.GetEntry("gamestate");
        var meta = _archive.GetEntry("meta");

        if (gameState == null || meta == null)
        {
            throw new InvalidDataException("Save file is missing required entries");
        }

        using (var gameStateStream = gameState.Open())
        {
            using (var metaStream = meta.Open())
            {
                return new GameSaveDocuments(
                    meta: GameStateDocument.Parse(metaStream),
                    gameState: GameStateDocument.Parse(gameStateStream)
                );
            }

        }

    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _archive.Dispose();
    }
} 