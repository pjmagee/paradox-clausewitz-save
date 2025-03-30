using MageeSoft.PDX.CE.Cli.Games;

namespace MageeSoft.PDX.CE.Cli.Services;

/// <summary>
/// Interface for providers that can locate and process save files for specific games
/// </summary>
public interface IGameFilesProvider
{
    /// <summary>
    /// The game type this provider supports
    /// </summary>
    GameType GameType { get; }
    
    /// <summary>
    /// The game name as it would appear in command line arguments
    /// </summary>
    string GameName { get; }
    
    /// <summary>
    /// The file extension for save files (including period)
    /// </summary>
    string SaveFileExtension { get; }
    
    /// <summary>
    /// Find all save files for this game
    /// </summary>
    /// <returns>Collection of found save files</returns>
    IEnumerable<FileInfo> FindSaveFiles();
    
    /// <summary>
    /// Get a summary of a save file
    /// </summary>
    /// <param name="saveFile">The save file to analyze</param>
    /// <returns>A summary object with details about the save</returns>
    SaveSummary GetSaveSummary(FileInfo saveFile);
    
    /// <summary>
    /// Converts a save file to JSON format
    /// </summary>
    /// <param name="saveFile">The save file to convert</param>
    /// <returns>JSON string representation</returns>
    string GetSummaryAsJson(FileInfo saveFile);
} 