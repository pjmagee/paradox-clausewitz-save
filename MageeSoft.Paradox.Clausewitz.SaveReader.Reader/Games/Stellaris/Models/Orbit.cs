using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an orbit in the game state.
/// </summary>
public record Orbit
{
    /// <summary>
    /// Gets or sets the orbitable.
    /// </summary>
    public required Orbitable Orbitable { get; init; }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Default instance of Orbit.
    /// </summary>
    public static Orbit Default => new()
    {
        Orbitable = Orbitable.Default,
        Index = -1
    };

    /// <summary>
    /// Loads an orbit from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the orbit data.</param>
    /// <returns>A new Orbit instance.</returns>
    public static Orbit? Load(SaveObject saveObject)
    {
        if (saveObject.Properties.Length == 0)
        {
            return Default;
        }

        SaveObject? orbitableObj;
        if (!saveObject.TryGetSaveObject("orbitable", out orbitableObj) || orbitableObj == null)
        {
            return null;
        }
        var orbitable = Orbitable.Load(orbitableObj);
        if (orbitable == null)
        {
            return null;
        }

        int index = -1;
        if (!saveObject.TryGetInt("index", out index))
        {
            index = -1;
        }

        return new Orbit
        {
            Orbitable = orbitable,
            Index = index
        };
    }
}






