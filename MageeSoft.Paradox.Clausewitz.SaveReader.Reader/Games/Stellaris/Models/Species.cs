using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a species in the game state.
/// </summary>
public record Species
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required LocalizedText Name { get; init; }

    /// <summary>
    /// Gets or sets the plural name.
    /// </summary>
    public required LocalizedText NamePlural { get; init; }

    /// <summary>
    /// Gets or sets the adjective.
    /// </summary>
    public required LocalizedText Adjective { get; init; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    public required string Class { get; init; }

    /// <summary>
    /// Gets or sets the portrait.
    /// </summary>
    public required string Portrait { get; init; }

    /// <summary>
    /// Gets or sets the name list.
    /// </summary>
    public required string NameList { get; init; }

    /// <summary>
    /// Gets or sets the traits.
    /// </summary>
    public required ImmutableArray<string> Traits { get; init; }

    /// <summary>
    /// Gets or sets the home planet ID.
    /// </summary>
    public required long HomePlanet { get; init; }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    public required string Gender { get; init; }

    /// <summary>
    /// Gets or sets the extra trait points.
    /// </summary>
    public required int ExtraTraitPoints { get; init; }

    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    public required ImmutableDictionary<string, long> Flags { get; init; }

    /// <summary>
    /// Gets or sets the name data.
    /// </summary>
    public required string NameData { get; init; }

    /// <summary>
    /// Gets or sets the base reference.
    /// </summary>
    public string? BaseRef { get; init; }
} 






