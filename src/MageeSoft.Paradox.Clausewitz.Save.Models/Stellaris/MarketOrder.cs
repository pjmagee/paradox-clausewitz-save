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
    public long? Id { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public Owner? Owner { get;set; }

    /// <summary>
    /// Gets or sets the resource.
    /// </summary>
    public string? Resource { get;set; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public float? Amount { get;set; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public float? Price { get;set; }

    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public string? Date { get;set; }
} 






