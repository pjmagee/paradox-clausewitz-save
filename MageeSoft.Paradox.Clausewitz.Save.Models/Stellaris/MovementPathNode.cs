namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a movement path node in the game state.
/// </summary>
public record MovementPathNode
{
    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public required Coordinate Coordinate { get; init; }

    /// <summary>
    /// Gets or sets the FTL type.
    /// </summary>
    public required string Ftl { get; init; }
}