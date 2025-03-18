using System.Collections.Immutable;


namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a construction in the game state.
/// </summary>
public record Construction
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the planet.
    /// </summary>
    public required int Planet { get; init; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public required float Progress { get; init; }

    /// <summary>
    /// Gets or sets whether the construction is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public ImmutableDictionary<string, float> Resources { get; init; }
} 






