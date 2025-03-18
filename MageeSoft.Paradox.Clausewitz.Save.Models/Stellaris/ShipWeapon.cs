namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a ship weapon in the game state.
/// </summary>
public record ShipWeapon
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the template.
    /// </summary>
    public required string Template { get; init; }
}







