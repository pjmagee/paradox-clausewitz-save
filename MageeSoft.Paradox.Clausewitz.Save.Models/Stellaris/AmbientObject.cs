namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an ambient object in the game state.
/// </summary>
public class AmbientObject
{
    /// <summary>
    /// Gets or sets the ambient object ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the ambient object.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the ambient object is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the coordinate of the ambient object.
    /// </summary>
    public required Coordinate Coordinate { get; init; }

    /// <summary>
    /// Gets or sets the data type of the ambient object.
    /// </summary>
    public required string Data { get; init; }

    /// <summary>
    /// Gets or sets the properties of the ambient object.
    /// </summary>
    public required AmbientObjectProperties Properties { get; init; }
}