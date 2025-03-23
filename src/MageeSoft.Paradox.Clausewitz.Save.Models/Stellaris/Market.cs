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
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public Owner Owner { get;set; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public ImmutableArray<MarketResource> Resources { get;set; }

    /// <summary>
    /// Gets or sets the orders.
    /// </summary>
    public ImmutableArray<MarketOrder> Orders { get;set; }

    /// <summary>
    /// Gets or sets the history.
    /// </summary>
    public ImmutableArray<MarketHistory> History { get;set; }
} 






