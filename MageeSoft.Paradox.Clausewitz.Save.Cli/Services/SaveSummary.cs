using System.Text.Json.Serialization;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Games;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services;

public class SaveSummary
{
    [JsonPropertyName("gameType")]
    public GameType GameType { get; set; }
    
    [JsonPropertyName("gameName")]
    public string GameName => GameType.ToString();
    
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;
    
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }
    
    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; }
    
    [JsonPropertyName("gameVersion")]
    public string Version { get; set; } = string.Empty;
    
    [JsonPropertyName("ironman")]
    public bool Ironman { get; set; }
    
    [JsonPropertyName("hasError")]
    public bool HasError { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    // Additional properties can be added as needed for specific games
} 