namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a ship hull section in the game state.
/// </summary>
public record ShipHullSection
{
    /// <summary>
    /// Gets or sets the template.
    /// </summary>
    public required string Template { get; init; }

    /// <summary>
    /// Gets or sets the slot.
    /// </summary>
    public required string Slot { get; init; }
}







