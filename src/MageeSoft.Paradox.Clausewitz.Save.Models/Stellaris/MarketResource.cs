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
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public float Price { get;set; }

    /// <summary>
    /// Gets or sets the demand.
    /// </summary>
    public float Demand { get;set; }

    /// <summary>
    /// Gets or sets the supply.
    /// </summary>
    public float Supply { get;set; }
} 






