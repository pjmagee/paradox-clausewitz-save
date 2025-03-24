using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents market history in the game state.
/// </summary>
[SaveModel]
public partial class MarketHistory
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public string? Date { get;set; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public List<MarketResource>? Resources { get;set; }
} 






