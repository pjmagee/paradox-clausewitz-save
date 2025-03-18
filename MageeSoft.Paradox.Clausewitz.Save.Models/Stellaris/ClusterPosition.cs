using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents the position of a cluster in the game state.
/// </summary>
public record ClusterPosition
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    [SaveScalar("x")]
    public required float X { get; init; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    [SaveScalar("y")]
    public required float Y { get; init; }

    /// <summary>
    /// Gets or sets the origin ID.
    /// </summary>
    [SaveScalar("origin")]
    public required long Origin { get; init; }

    /// <summary>
    /// Gets or sets whether the position is randomized.
    /// </summary>
    [SaveScalar("randomized")]
    public required bool Randomized { get; init; }

    /// <summary>
    /// Gets or sets the visual height.
    /// </summary>
    [SaveScalar("visual_height")]
    public required float VisualHeight { get; init; }
}