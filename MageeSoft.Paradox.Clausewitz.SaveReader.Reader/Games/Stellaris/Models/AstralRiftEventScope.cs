using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents the scope of an astral rift event.
/// </summary>
public record AstralRiftEventScope
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
    /// Gets or sets the random values.
    /// </summary>
    public required ImmutableArray<long> Random { get; init; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public required bool RandomAllowed { get; init; }

    /// <summary>
    /// Default instance of AstralRiftEventScope.
    /// </summary>
    public static AstralRiftEventScope Default => new()
    {
        Type = string.Empty,
        Id = 0,
        OpenerId = 0,
        Random = ImmutableArray<long>.Empty,
        RandomAllowed = false
    };
}