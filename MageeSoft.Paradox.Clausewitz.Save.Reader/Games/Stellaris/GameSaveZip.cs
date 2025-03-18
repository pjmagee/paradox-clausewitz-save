using System.IO.Compression;
using MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

namespace MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;

/// <summary>
/// Represents a game save zip file. (.sav)
/// </summary>          
public class GameSaveZip : IDisposable
{
    const string GameStateFileName = "gamestate";
    const string MetaFileName = "meta";

    readonly ZipArchive _archive;

    public GameSaveZip(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        _archive = new ZipArchive(stream, ZipArchiveMode.Read);
    }

    public GameSaveDocuments GetDocuments()
    {
        var gameState = _archive.GetEntry(GameStateFileName);
        var meta = _archive.GetEntry(MetaFileName);

        if (gameState == null || meta == null)
        {
            throw new InvalidDataException("Save file is missing required entries");
        }

        using (var gameStateStream = gameState.Open())
        {
            using (var metaStream = meta.Open())
            {
                return new GameSaveDocuments(
                    metaDocument: GameStateDocument.Parse(metaStream),
                    gameStateDocument: GameStateDocument.Parse(gameStateStream)
                );
            }
        }
    }

    public void Dispose()
    {
        _archive.Dispose();
    }
} 