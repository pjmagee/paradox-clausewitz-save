namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a weapon in a ship section.
/// </summary>
public class ShipWeapon
{
    /// <summary>
    /// Gets or sets the index of the weapon.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the template of the weapon.
    /// </summary>
    public string Template { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the component slot where this weapon is placed.
    /// </summary>
    public string WeaponSlot { get; set; } = string.Empty;
}