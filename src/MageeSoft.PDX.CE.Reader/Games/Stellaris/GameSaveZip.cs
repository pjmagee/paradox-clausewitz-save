using System.IO.Compression;

namespace MageeSoft.PDX.CE.Reader.Games.Stellaris;

/// <summary>
/// Represents a game save zip file. (.sav)
/// </summary>          
public class GameSaveZip : IDisposable
{
    const string GameStateFileName = "gamestate";
    const string MetaFileName = "meta";

    readonly ZipArchive _archive;

    private GameSaveZip(Stream stream)
    {
        // ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        _archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
    }
    
    public static GameSaveZip Open(string filePath)
    {
        //ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));
        var stream = File.OpenRead(filePath);
        return new GameSaveZip(stream);
    }
    
    public static GameSaveZip Open(FileInfo fileInfo)
    {
        //ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));
        var stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Delete);
        return new GameSaveZip(stream);
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
        try
        {
            _archive.Dispose();    
        }
        catch (ObjectDisposedException)
        {
            // Ignore if already disposed
        }
    }
} 