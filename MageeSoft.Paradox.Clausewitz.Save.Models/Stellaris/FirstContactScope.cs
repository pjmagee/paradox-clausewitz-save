using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a first contact scope in the game state.
/// </summary>
public class FirstContactScope
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
    /// Gets or sets the opener ID.
    /// </summary>
    public required long OpenerId { get; init; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public required bool RandomAllowed { get; init; }

    /// <summary>
    /// Gets or sets the random value.
    /// </summary>
    public required float Random { get; init; }

    /// <summary>
    /// Gets or sets the root.
    /// </summary>
    public required long Root { get; init; }

    /// <summary>
    /// Gets or sets the from value.
    /// </summary>
    public required long From { get; init; }

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






