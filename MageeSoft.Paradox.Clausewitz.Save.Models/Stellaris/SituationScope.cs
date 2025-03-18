using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a situation scope in the game state.
/// </summary>
public record SituationScope
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public required long Country { get; init; }

    /// <summary>
    /// Gets or sets the systems.
    /// </summary>
    public required ImmutableArray<long> Systems { get; init; }

    /// <summary>
    /// Gets or sets the planets.
    /// </summary>
    public required ImmutableArray<long> Planets { get; init; }

    /// <summary>
    /// Gets or sets the fleets.
    /// </summary>
    public required ImmutableArray<long> Fleets { get; init; }
} 






