using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a species in the game state.
/// </summary>
[SaveModel]
public partial class Species
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required LocalizedText Name { get;set; }

    /// <summary>
    /// Gets or sets the plural name.
    /// </summary>
    public required LocalizedText NamePlural { get;set; }

    /// <summary>
    /// Gets or sets the adjective.
    /// </summary>
    public required LocalizedText Adjective { get;set; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    public required string Class { get;set; }

    /// <summary>
    /// Gets or sets the portrait.
    /// </summary>
    public required string Portrait { get;set; }

    /// <summary>
    /// Gets or sets the name list.
    /// </summary>
    public required string NameList { get;set; }

    /// <summary>
    /// Gets or sets the traits.
    /// </summary>
    public required ImmutableArray<string> Traits { get;set; }

    /// <summary>
    /// Gets or sets the home planet ID.
    /// </summary>
    public required long HomePlanet { get;set; }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    public required string Gender { get;set; }

    /// <summary>
    /// Gets or sets the extra trait points.
    /// </summary>
    public required int ExtraTraitPoints { get;set; }

    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    public required ImmutableDictionary<string, long> Flags { get;set; }

    /// <summary>
    /// Gets or sets the name data.
    /// </summary>
    public required string NameData { get;set; }

    /// <summary>
    /// Gets or sets the base reference.
    /// </summary>
    public string? BaseRef { get;set; }
} 






