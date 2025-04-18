using System.Text.Json.Serialization;

namespace MageeSoft.PDX.CE.Cli.Services;

public class SaveSummary
{
    [JsonPropertyName("gameType")] public GameType GameType { get; set; }

    [JsonPropertyName("fileName")] public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("fileSize")] public long FileSize { get; set; }

    [JsonPropertyName("lastModified")] public DateTime LastModified { get; set; }

    [JsonPropertyName("gameVersion")] public string Version { get; set; } = string.Empty;

    [JsonPropertyName("ironman")] public bool Ironman { get; set; }

    [JsonPropertyName("hasError")] public bool HasError { get; set; }

    [JsonPropertyName("error")] public string? Error { get; set; }
}