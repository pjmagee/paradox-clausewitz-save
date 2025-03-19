using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a market in the game state.
/// </summary>
[SaveModel]
public partial class Market
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required Owner Owner { get;set; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public required ImmutableArray<MarketResource> Resources { get;set; }

    /// <summary>
    /// Gets or sets the orders.
    /// </summary>
    public required ImmutableArray<MarketOrder> Orders { get;set; }

    /// <summary>
    /// Gets or sets the history.
    /// </summary>
    public required ImmutableArray<MarketHistory> History { get;set; }
} 






