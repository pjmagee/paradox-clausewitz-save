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
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the opener ID.
    /// </summary>
    public long OpenerId { get;set; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public bool RandomAllowed { get;set; }

    /// <summary>
    /// Gets or sets the random value.
    /// </summary>
    public float Random { get;set; }

    /// <summary>
    /// Gets or sets the root.
    /// </summary>
    public long Root { get;set; }

    /// <summary>
    /// Gets or sets the from value.
    /// </summary>
    public long From { get;set; }

    /// <summary>
    /// Gets or sets the systems.
    /// </summary>
    public ImmutableArray<long> Systems { get;set; }

    /// <summary>
    /// Gets or sets the planets.
    /// </summary>
    public ImmutableArray<long> Planets { get;set; }

    /// <summary>
    /// Gets or sets the fleets.
    /// </summary>
    public ImmutableArray<long> Fleets { get;set; }
}






