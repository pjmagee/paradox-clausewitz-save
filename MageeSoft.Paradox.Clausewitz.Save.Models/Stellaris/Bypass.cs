namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a bypass in the game state.
/// </summary>
public record Bypass
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets whether the bypass is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required long Owner { get; init; }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Gets or sets whether the bypass is locked.
    /// </summary>
    public required bool IsLocked { get; init; }

    /// <summary>
    /// Gets or sets the days left.
    /// </summary>
    public required int DaysLeft { get; init; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public Position Position { get; init; }
} 