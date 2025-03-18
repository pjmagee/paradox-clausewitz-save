using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents market history in the game state.
/// </summary>
public record MarketHistory
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public required ImmutableArray<MarketResource> Resources { get; init; }
} 






