
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a planet in the game state.
/// </summary>
public record Planet
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    public required string Class { get; init; }

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    public required int Size { get; init; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required long Owner { get; init; }

    /// <summary>
    /// Gets or sets the original owner.
    /// </summary>
    public required long OriginalOwner { get; init; }

    /// <summary>
    /// Gets or sets the controller.
    /// </summary>
    public required long Controller { get; init; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public required Position Position { get; init; }

    /// <summary>
    /// Gets or sets the deposits.
    /// </summary>
    public required ImmutableArray<long> Deposits { get; init; }

    /// <summary>
    /// Gets or sets the pops.
    /// </summary>
    public required ImmutableArray<long> Pops { get; init; }

    /// <summary>
    /// Gets or sets the buildings.
    /// </summary>
    public required ImmutableArray<long> Buildings { get; init; }

    /// <summary>
    /// Gets or sets the districts.
    /// </summary>
    public required ImmutableArray<string> Districts { get; init; }

    /// <summary>
    /// Gets or sets the moons.
    /// </summary>
    public required ImmutableArray<long> Moons { get; init; }

    /// <summary>
    /// Gets or sets whether this planet is a moon.
    /// </summary>
    public required bool IsMoon { get; init; }

    /// <summary>
    /// Gets or sets whether this planet has a ring.
    /// </summary>
    public required bool HasRing { get; init; }

    /// <summary>
    /// Gets or sets the moon of planet ID.
    /// </summary>
    public long? MoonOf { get; init; }

    /// <summary>
    /// Gets or sets the colonize date.
    /// </summary>
    public DateOnly? ColonizeDate { get; init; }
}
