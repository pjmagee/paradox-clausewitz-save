using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

public record Building
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int RuinTime { get; init; }
} 






