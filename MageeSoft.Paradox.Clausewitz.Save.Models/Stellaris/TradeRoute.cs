using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a trade route in the game state.
/// </summary>
[SaveModel]
public partial class TradeRoute
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public Owner Owner { get;set; }

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    public ImmutableArray<Position> Path { get;set; }
} 






