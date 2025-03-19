namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a market resource in the game state.
/// </summary>  
[SaveModel]
public partial class MarketResource
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public required float Price { get;set; }

    /// <summary>
    /// Gets or sets the demand.
    /// </summary>
    public required float Demand { get;set; }

    /// <summary>
    /// Gets or sets the supply.
    /// </summary>
    public required float Supply { get;set; }
} 






