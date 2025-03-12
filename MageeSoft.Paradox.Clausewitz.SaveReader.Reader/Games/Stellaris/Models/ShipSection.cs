namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a section of a ship design.
/// </summary>
public class ShipSection
{
    /// <summary>
    /// Gets or sets the template of the section.
    /// </summary>
    public string Template { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slot where this section is placed.
    /// </summary>
    public string Slot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the components in this section.
    /// </summary>
    public IReadOnlyList<ShipComponent> Components { get; set; } = new List<ShipComponent>();
}