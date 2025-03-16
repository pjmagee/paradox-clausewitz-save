using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a ship component in the game state.
/// </summary>
public record ShipComponent
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
    /// Default instance of ShipComponent.
    /// </summary>
    public static ShipComponent Default => new()
    {
        Name = string.Empty,
        Template = string.Empty
    };

    /// <summary>
    /// Loads a ship component from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the ship component data.</param>
    /// <returns>A new ShipComponent instance.</returns>
    public static ShipComponent? Load(SaveObject saveObject)
    {
        string name;
        string template;

        if (!saveObject.TryGetString("name", out name) ||
            !saveObject.TryGetString("template", out template))
        {
            return null;
        }

        return new ShipComponent
        {
            Name = name,
            Template = template
        };
    }
}







