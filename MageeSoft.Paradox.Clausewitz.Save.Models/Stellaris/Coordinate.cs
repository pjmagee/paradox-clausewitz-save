namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a coordinate in the game state.
/// </summary>
[SaveModel]
public partial class Coordinate
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public float X { get;set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public float Y { get;set; }

    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    public float Z { get;set; }

    /// <summary>
    /// Gets or sets the origin.
    /// </summary>
    public long Origin { get;set; }

    /// <summary>
    /// Gets or sets a value indicating whether the coordinate is randomized.
    /// </summary>
    public bool Randomized { get;set; }

    /// <summary>
    /// Gets or sets the visual height of the coordinate.
    /// </summary>
    public float VisualHeight { get;set; }
}