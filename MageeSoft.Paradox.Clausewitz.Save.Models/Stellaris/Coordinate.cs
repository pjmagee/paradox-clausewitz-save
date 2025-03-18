namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

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
}