namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

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

} 






