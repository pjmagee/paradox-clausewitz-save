namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a position in the game state.
/// </summary>
[SaveModel]
public partial class Position
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    [SaveScalar("x")]
    public float? X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    [SaveScalar("y")]
    public float? Y { get; set; }

    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    [SaveScalar("z")]
    public float? Z { get; set; }

} 






