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
    public required float X { get;set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public required float Y { get;set; }

    /// <summary>
    /// Gets or sets the Z coordinate.
    /// </summary>
    public required float Z { get;set; }

} 






