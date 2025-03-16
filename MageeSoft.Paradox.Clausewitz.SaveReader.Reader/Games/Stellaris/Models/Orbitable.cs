using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an orbitable object in the game state.
/// </summary>
public record Orbitable
{
    /// <summary>
    /// Gets or sets the planet ID.
    /// </summary>
    public required long Planet { get; init; }

    /// <summary>
    /// Default instance of Orbitable.
    /// </summary>
    public static Orbitable Default => new()
    {
        Planet = 4294967295
    };

    /// <summary>
    /// Loads an orbitable from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the orbitable data.</param>
    /// <returns>A new Orbitable instance.</returns>
    public static Orbitable? Load(SaveObject saveObject)
    {
        long planet;
        if (!saveObject.TryGetLong("planet", out planet))
        {
            planet = 4294967295;
        }

        return new Orbitable
        {
            Planet = planet
        };
    }
}







