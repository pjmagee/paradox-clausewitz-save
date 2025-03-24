namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a movement path node in the game state.
/// </summary>
[SaveModel]
public partial class MovementPathNode
{
    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public Coordinate? Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the FTL type.
    /// </summary>
    public string? Ftl { get;set; }
}