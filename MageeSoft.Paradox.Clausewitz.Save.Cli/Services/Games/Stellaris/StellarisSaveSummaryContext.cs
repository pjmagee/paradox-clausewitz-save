using System.Text.Json.Serialization;

namespace MageeSoft.Paradox.Clausewitz.Save.Cli.Services.Games.Stellaris;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(StellarisSaveSummary))]
public partial class StellarisSaveSummaryContext : JsonSerializerContext
{
    
}