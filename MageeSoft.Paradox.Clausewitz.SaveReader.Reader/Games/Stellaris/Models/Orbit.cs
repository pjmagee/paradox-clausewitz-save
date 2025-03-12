using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an orbit in the game state.
/// </summary>
public class Orbit
{
    /// <summary>
    /// Gets or sets the orbitable ID.
    /// </summary>
    public long Orbitable { get; set; }

    /// <summary>
    /// Gets or sets the planet ID.
    /// </summary>
    public long Planet { get; set; }

    /// <summary>
    /// Gets or sets the orbit index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Loads an orbit from a SaveElement.
    /// </summary>
    /// <param name="clausewitzElement">The SaveElement containing the orbit data.</param>
    /// <returns>A new Orbit instance.</returns>
    public static Orbit Load(SaveElement clausewitzElement)
    {
        var orbit = new Orbit();
        var orbitObj = clausewitzElement as SaveObject;
        if (orbitObj != null)
        {
            foreach (var property in orbitObj.Properties)
            {
                switch (property.Key)
                {
                    case "orbitable" when property.Value is Scalar<long> orbitableScalar:
                        orbit.Orbitable = orbitableScalar.Value;
                        break;
                    case "planet" when property.Value is Scalar<long> planetScalar:
                        orbit.Planet = planetScalar.Value;
                        break;
                    case "index" when property.Value is Scalar<int> indexScalar:
                        orbit.Index = indexScalar.Value;
                        break;
                }
            }
        }

        return orbit;
    }
}