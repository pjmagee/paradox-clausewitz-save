using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

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

    /// <summary>
    /// Default instance of ShipHullSection.
    /// </summary>
    public static ShipHullSection Default => new()
    {
        Template = string.Empty,
        Slot = string.Empty
    };

    /// <summary>
    /// Loads a ship hull section from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the ship hull section data.</param>
    /// <returns>A new ShipHullSection instance.</returns>
    public static ShipHullSection? Load(SaveObject saveObject)
    {
        string template;
        string slot;

        if (!saveObject.TryGetString("template", out template) ||
            !saveObject.TryGetString("slot", out slot))
        {
            return null;
        }

        return new ShipHullSection
        {
            Template = template,
            Slot = slot
        };
    }
}







