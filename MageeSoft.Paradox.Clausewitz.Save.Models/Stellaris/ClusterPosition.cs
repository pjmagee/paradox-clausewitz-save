namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents the position of a cluster in the game state.
/// </summary>
[SaveModel]
public partial class ClusterPosition
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
    /// Gets or sets the origin ID.
    /// </summary>
    [SaveScalar("origin")]
    public long Origin { get;set; }

    /// <summary>
    /// Gets or sets whether the position is randomized.
    /// </summary>
    [SaveScalar("randomized")]
    public bool Randomized { get;set; }

    /// <summary>
    /// Gets or sets the visual height.
    /// </summary>
    [SaveScalar("visual_height")]
    public float VisualHeight { get;set; }
}