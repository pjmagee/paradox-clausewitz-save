using System.Text.Json.Serialization;
using MageeSoft.PDX.CE.Cli.Services.Games.Stellaris;

namespace MageeSoft.PDX.CE.Cli.Services;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(SaveSummary))]
[JsonSerializable(typeof(StellarisSaveSummary))]
internal partial class CliJsonSerializerContext : JsonSerializerContext
{
    
}