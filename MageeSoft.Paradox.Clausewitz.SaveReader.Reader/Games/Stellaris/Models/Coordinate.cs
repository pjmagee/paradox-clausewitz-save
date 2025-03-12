using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a coordinate in the game state.
/// </summary>
public class Coordinate
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    /// Gets or sets the origin.
    /// </summary>
    public long Origin { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the coordinate is randomized.
    /// </summary>
    public bool Randomized { get; set; }

    /// <summary>
    /// Gets or sets the visual height of the coordinate.
    /// </summary>
    public float VisualHeight { get; set; }

    /// <summary>
    /// Creates a new instance of the Coordinate class.
    /// </summary>
    public Coordinate() { }

    /// <summary>
    /// Creates a new instance of the Coordinate class with the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public Coordinate(float x, float y, float z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Loads a coordinate from a SaveElement.
    /// </summary>
    /// <param name="element">The SaveElement containing the coordinate data.</param>
    /// <returns>A new Coordinate instance.</returns>
    public static Coordinate Load(SaveElement element)
    {
        var coordinate = new Coordinate();
        var coordObj = element as SaveObject;
        if (coordObj != null)
        {
            foreach (var property in coordObj.Properties)
            {
                switch (property.Key)
                {
                    case "x":
                        if (property.Value?.TryGetScalar<float>(out var x) == true)
                        {
                            coordinate.X = x;
                        }
                        break;
                    case "y":
                        if (property.Value?.TryGetScalar<float>(out var y) == true)
                        {
                            coordinate.Y = y;
                        }
                        break;
                    case "z":
                        if (property.Value?.TryGetScalar<float>(out var z) == true)
                        {
                            coordinate.Z = z;
                        }
                        break;
                    case "origin":
                        if (property.Value?.TryGetScalar<long>(out var origin) == true)
                        {
                            coordinate.Origin = origin;
                        }
                        break;
                    case "randomized":
                        if (property.Value?.TryGetScalar<bool>(out var randomized) == true)
                        {
                            coordinate.Randomized = randomized;
                        }
                        break;
                    case "visual_height":
                        if (property.Value?.TryGetScalar<float>(out var visualHeight) == true)
                        {
                            coordinate.VisualHeight = visualHeight;
                        }
                        break;
                }
            }
        }

        return coordinate;
    }
}