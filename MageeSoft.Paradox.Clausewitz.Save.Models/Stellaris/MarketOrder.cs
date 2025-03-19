namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a market order in the game state.
/// </summary>  
[SaveModel]
public partial class MarketOrder
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
    /// Gets or sets the resource.
    /// </summary>
    public required string Resource { get;set; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public required float Amount { get;set; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public required float Price { get;set; }

    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public required string Date { get;set; }
} 






