namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a section of a ship's hull.
/// </summary>
public class ShipHullSection
{
    /// <summary>
    /// Gets or sets the design of the section.
    /// </summary>
    public string Design { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slot where this section is placed.
    /// </summary>
    public string SectionSlot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the weapons in this section.
    /// </summary>
    public IReadOnlyList<ShipWeapon> Weapons { get; set; } = new List<ShipWeapon>();
}