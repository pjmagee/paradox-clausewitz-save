namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents debris in the game state.
/// </summary>
public record Debris
{
    /// <summary>
    /// Gets or sets the debris ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the country ID that owns the debris.
    /// </summary>
    public required long Owner { get; init; }

    /// <summary>
    /// Gets or sets the coordinate of the debris.
    /// </summary>
    public required Position Position { get; init; }

    /// <summary>
    /// Gets or sets the type of the debris.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets whether the debris is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets whether the debris is visible.
    /// </summary>
    public required bool IsVisible { get; init; }

    /// <summary>
    /// Gets or sets whether the debris is hostile.
    /// </summary>
    public required bool IsHostile { get; init; }

} 






