using System.Text.Json.Serialization;
using MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Stellaris;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(SaveSummary))]
[JsonSerializable(typeof(StellarisSaveSummary))]
internal partial class CliJsonSerializerContext : JsonSerializerContext
{
    
}