using System.Text.Json.Serialization;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services;

/// <summary>
/// Information about a save file for display purposes
/// </summary>
public class SaveFileInfo
{
    /// <summary>
    /// Sequential number for reference
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }
    
    /// <summary>
    /// The game type this save file belongs to
    /// </summary>
    [JsonPropertyName("gameType")]
    public GameType GameType { get; set; }
    
    /// <summary>
    /// Human-readable game name
    /// </summary>
    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full path to the save file
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// File name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Parent directory name
    /// </summary>
    [JsonPropertyName("directory")]
    public string Directory { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }
    
    /// <summary>
    /// Last modified timestamp
    /// </summary>
    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; }
} 