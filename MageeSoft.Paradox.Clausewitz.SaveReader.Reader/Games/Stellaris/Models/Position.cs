using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a position in the game state.
/// </summary>
public record Position
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public required float X { get; init; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public required float Y { get; init; }

    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    public required float Z { get; init; }

    /// <summary>
    /// Creates a new instance of Position with default values.
    /// </summary>
    public Position()
    {
        X = 0f;
        Y = 0f;
        Z = 0f;
    }

    /// <summary>
    /// Default instance of Position.
    /// </summary>
    public static Position Default { get; } = new()
    {
        X = 0f,
        Y = 0f,
        Z = 0f
    };

    /// <summary>
    /// Loads a position from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the position data.</param>
    /// <returns>A new Position instance.</returns>
    public static Position? Load(SaveObject saveObject)
    {
        if (!saveObject.TryGetFloat("x", out var x) ||
            !saveObject.TryGetFloat("y", out var y))
        {
            return null;
        }

        return new Position
        {
            X = x,
            Y = y,
            Z = saveObject.TryGetFloat("z", out var z) ? z : 0f
        };
    }
} 






