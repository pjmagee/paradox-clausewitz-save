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
    [SaveScalar("x")]
    public float X { get;set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    [SaveScalar("y")]
    public float Y { get;set; }

    /// <summary>
    /// Gets or sets the origin.
    /// </summary>
    [SaveScalar("origin")]
    public long Origin { get;set; }

    /// <summary>
    /// Gets or sets a value indicating whether the coordinate is randomized.
    /// </summary>
    [SaveScalar("randomized")]
    public bool Randomized { get;set; }

    /// <summary>
    /// Gets or sets the visual height of the coordinate.
    /// </summary>
    [SaveScalar("visual_height")]
    public float VisualHeight { get;set; }
}