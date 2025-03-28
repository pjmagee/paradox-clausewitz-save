using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a situation scope in the game state.
/// </summary>
[SaveModel]
public partial class SituationScope
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long? Id { get;set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public long? Country { get;set; }

    /// <summary>
    /// Gets or sets the systems.
    /// </summary>
    public List<long>? Systems { get;set; }

    /// <summary>
    /// Gets or sets the planets.
    /// </summary>
    public List<long>? Planets { get;set; }

    /// <summary>
    /// Gets or sets the fleets.
    /// </summary>
    public List<long>? Fleets { get;set; }
} 






