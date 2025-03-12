namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a component in a ship section.
/// </summary>
public class ShipComponent
{
    /// <summary>
    /// Gets or sets the slot where this component is placed.
    /// </summary>
    public string Slot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template of the component.
    /// </summary>
    public string Template { get; set; } = string.Empty;
}