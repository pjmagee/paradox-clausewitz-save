namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a resource in the game state.
/// </summary>
[SaveModel]
public partial class Resource
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public float? Amount { get;set; }

} 






