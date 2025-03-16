using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a coordinate in the game state.
/// </summary>
public record Coordinate
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public float X { get; init; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public float Y { get; init; }

    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    public float Z { get; init; }

    /// <summary>
    /// Gets or sets the origin.
    /// </summary>
    public long Origin { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the coordinate is randomized.
    /// </summary>
    public bool Randomized { get; init; }

    /// <summary>
    /// Gets or sets the visual height of the coordinate.
    /// </summary>
    public float VisualHeight { get; init; }

    /// <summary>
    /// Gets the default instance of Coordinate.
    /// </summary>
    public static Coordinate Default => new Coordinate
    {
        X = 0f,
        Y = 0f,
        Z = 0f,
        Origin = 0,
        Randomized = false,
        VisualHeight = 0f
    };


    /// <summary>
    /// Loads a coordinate from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the coordinate data.</param>
    /// <returns>A new Coordinate instance.</returns>
    public static Coordinate? Load(SaveObject obj)
    {
        if (!obj.TryGetFloat("x", out var x) ||
            !obj.TryGetFloat("y", out var y))
        {
            return null;
        }

        return new Coordinate
        {
            X = x,
            Y = y,
            Z = obj.TryGetFloat("z", out var z) ? z : 0f,
            Origin = obj.TryGetLong("origin", out var origin) ? origin : 4294967295,
            Randomized = obj.TryGetBool("randomized", out var randomized) && randomized,
            VisualHeight = obj.TryGetFloat("visual_height", out var visualHeight) ? visualHeight : 0f
        };
    }
}