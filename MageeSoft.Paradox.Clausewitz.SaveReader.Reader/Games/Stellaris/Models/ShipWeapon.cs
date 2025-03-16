using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

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

    /// <summary>
    /// Default instance of ShipWeapon.
    /// </summary>
    public static ShipWeapon Default => new()
    {
        Name = string.Empty,
        Template = string.Empty
    };

    /// <summary>
    /// Loads a ship weapon from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the ship weapon data.</param>
    /// <returns>A new ShipWeapon instance.</returns>
    public static ShipWeapon? Load(SaveObject saveObject)
    {
        string name;
        string template;

        if (!saveObject.TryGetString("name", out name) ||
            !saveObject.TryGetString("template", out template))
        {
            return null;
        }

        return new ShipWeapon
        {
            Name = name,
            Template = template
        };
    }
}







