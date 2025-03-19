using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a planet in the game state.
/// </summary>
[SaveModel]
public partial class Planet
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [SaveScalar("name")]
    public string Name { get;set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    public required string Class { get;set; }

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    public required int Size { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required long Owner { get;set; }

    /// <summary>
    /// Gets or sets the original owner.
    /// </summary>
    public required long OriginalOwner { get;set; }

    /// <summary>
    /// Gets or sets the controller.
    /// </summary>
    public required long Controller { get;set; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public required Position Position { get;set; }

    /// <summary>
    /// Gets or sets the deposits.
    /// </summary>
    public required ImmutableArray<long> Deposits { get;set; }

    /// <summary>
    /// Gets or sets the pops.
    /// </summary>
    public required ImmutableArray<long> Pops { get;set; }

    /// <summary>
    /// Gets or sets the buildings.
    /// </summary>
    public required ImmutableArray<long> Buildings { get;set; }

    /// <summary>
    /// Gets or sets the districts.
    /// </summary>
    public required ImmutableArray<string> Districts { get;set; }

    /// <summary>
    /// Gets or sets the moons.
    /// </summary>
    public required ImmutableArray<long> Moons { get;set; }

    /// <summary>
    /// Gets or sets whether this planet is a moon.
    /// </summary>
    public required bool IsMoon { get;set; }

    /// <summary>
    /// Gets or sets whether this planet has a ring.
    /// </summary>
    public required bool HasRing { get;set; }

    /// <summary>
    /// Gets or sets the moon of planet ID.
    /// </summary>
    public long? MoonOf { get;set; }

    /// <summary>
    /// Gets or sets the colonize date.
    /// </summary>
    public DateOnly? ColonizeDate { get;set; }
}
