using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a sector in the game state.
/// </summary>
[SaveModel]
public partial class Sector
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required Owner Owner { get;set; }

    /// <summary>
    /// Gets or sets the planets.
    /// </summary>
    public required ImmutableArray<Planet> Planets { get;set; }

    
} 






