namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a ship hull section in the game state.
/// </summary>
[SaveModel]
public partial class ShipHullSection
{
    /// <summary>
    /// Gets or sets the template.
    /// </summary>
    public string? Template { get;set; }

    /// <summary>
    /// Gets or sets the slot.
    /// </summary>
    public string? Slot { get;set; }
}







