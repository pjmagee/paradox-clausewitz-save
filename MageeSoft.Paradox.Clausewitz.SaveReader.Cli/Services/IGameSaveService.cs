using MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Games;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Cli.Services;

/// <summary>
/// Interface for game-specific save file services.
/// </summary>
public interface IGameSaveService
{
    /// <summary>
    /// Gets the type of game this service handles.
    /// </summary>
    GameType GameType { get; }

    /// <summary>
    /// Gets the default save directory for this game.
    /// </summary>
    string DefaultSaveDirectory { get; }

    /// <summary>
    /// Gets the file extension for save files of this game.
    /// </summary>
    string SaveFileExtension { get; }

    /// <summary>
    /// Finds all save files for this game.
    /// </summary>
    /// <returns>A collection of save files.</returns>
    IEnumerable<FileInfo> FindSaveFiles();

    /// <summary>
    /// Validates if a file is a valid save file for this game.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>True if the file is valid, false otherwise.</returns>
    bool IsValidSaveFile(FileInfo file);
} 