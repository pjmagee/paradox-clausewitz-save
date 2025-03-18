namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a situation in the game state.
/// </summary>
public record Situation
{
    /// <summary>
    /// Gets or sets the situation ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the situation.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public required int Country { get; init; }

    /// <summary>
    /// Gets or sets the progress value.
    /// </summary>
    public required double Progress { get; init; }

    /// <summary>
    /// Gets or sets the last month progress value.
    /// </summary>
    public required double LastMonthProgress { get; init; }

    /// <summary>
    /// Gets or sets the approach value.
    /// </summary>
    public required string Approach { get; init; }
} 






