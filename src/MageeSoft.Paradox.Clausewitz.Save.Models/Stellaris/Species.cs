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
    public long? Id { get;set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public LocalizedText? Name { get;set; }

    /// <summary>
    /// Gets or sets the plural name.
    /// </summary>
    public LocalizedText? NamePlural { get;set; }

    /// <summary>
    /// Gets or sets the adjective.
    /// </summary>
    public LocalizedText? Adjective { get;set; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    public string? Class { get;set; }

    /// <summary>
    /// Gets or sets the portrait.
    /// </summary>
    public string? Portrait { get;set; }

    /// <summary>
    /// Gets or sets the name list.
    /// </summary>
    public string? NameList { get;set; }

    /// <summary>
    /// Gets or sets the traits.
    /// </summary>
    public List<string>? Traits { get;set; }

    /// <summary>
    /// Gets or sets the home planet ID.
    /// </summary>
    public long? HomePlanet { get;set; }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    public string? Gender { get;set; }

    /// <summary>
    /// Gets or sets the extra trait points.
    /// </summary>
    public int? ExtraTraitPoints { get;set; }

    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    public Dictionary<string, long>? Flags { get;set; }

    /// <summary>
    /// Gets or sets the name data.
    /// </summary>
    public string? NameData { get;set; }

    /// <summary>
    /// Gets or sets the base reference.
    /// </summary>
    public string? BaseRef { get;set; }
} 






