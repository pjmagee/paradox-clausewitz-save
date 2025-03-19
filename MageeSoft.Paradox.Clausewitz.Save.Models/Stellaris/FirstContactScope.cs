using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a first contact scope in the game state.
/// </summary>
[SaveModel]
public partial class FirstContactScope
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the opener ID.
    /// </summary>
    public required long OpenerId { get;set; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public required bool RandomAllowed { get;set; }

    /// <summary>
    /// Gets or sets the random value.
    /// </summary>
    public required float Random { get;set; }

    /// <summary>
    /// Gets or sets the root.
    /// </summary>
    public required long Root { get;set; }

    /// <summary>
    /// Gets or sets the from value.
    /// </summary>
    public required long From { get;set; }

    /// <summary>
    /// Gets or sets the systems.
    /// </summary>
    public required ImmutableArray<long> Systems { get;set; }

    /// <summary>
    /// Gets or sets the planets.
    /// </summary>
    public required ImmutableArray<long> Planets { get;set; }

    /// <summary>
    /// Gets or sets the fleets.
    /// </summary>
    public required ImmutableArray<long> Fleets { get;set; }
}






