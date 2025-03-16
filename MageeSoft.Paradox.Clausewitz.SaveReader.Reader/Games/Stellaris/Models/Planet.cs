using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

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

    /// <summary>
    /// Default instance of Planet.
    /// </summary>
    public static Planet Default => new()
    {
        Id = 0,
        Name = string.Empty,
        Class = string.Empty,
        Size = 0,
        Owner = 0,
        OriginalOwner = 0,
        Controller = 0,
        Position = Models.Position.Default,
        Deposits = ImmutableArray<long>.Empty,
        Pops = ImmutableArray<long>.Empty,
        Buildings = ImmutableArray<long>.Empty,
        Districts = ImmutableArray<string>.Empty,
        Moons = ImmutableArray<long>.Empty,
        IsMoon = false,
        HasRing = false,
        MoonOf = null,
        ColonizeDate = null
    };

    /// <summary>
    /// Loads a planet from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the planet data.</param>
    /// <returns>A new Planet instance.</returns>
    public static Planet? Load(SaveObject saveObject)
    {
        long id;
        string name;
        string planetClass;
        int size;
        Position? position;

        if (!saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetString("name", out name) ||
            !saveObject.TryGetString("class", out planetClass) ||
            !saveObject.TryGetInt("size", out size))
        {
            return null;
        }

        SaveObject? positionObj;
        if (!saveObject.TryGetSaveObject("position", out positionObj) || positionObj == null ||
            (position = Position.Load(positionObj)) == null)
        {
            return null;
        }

        long owner = 0;
        saveObject.TryGetLong("owner", out owner);

        long originalOwner = 0;
        saveObject.TryGetLong("original_owner", out originalOwner);

        long controller = 0;
        saveObject.TryGetLong("controller", out controller);

        SaveArray? depositsArray;
        var deposits = saveObject.TryGetSaveArray("deposits", out depositsArray) && depositsArray != null
            ? depositsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetLong("value", out var value) ? value : 0)
                .ToImmutableArray()
            : ImmutableArray<long>.Empty;

        SaveArray? popsArray;
        var pops = saveObject.TryGetSaveArray("pops", out popsArray) && popsArray != null
            ? popsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetLong("value", out var value) ? value : 0)
                .ToImmutableArray()
            : ImmutableArray<long>.Empty;

        SaveArray? buildingsArray;
        var buildings = saveObject.TryGetSaveArray("buildings", out buildingsArray) && buildingsArray != null
            ? buildingsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetLong("value", out var value) ? value : 0)
                .ToImmutableArray()
            : ImmutableArray<long>.Empty;

        SaveArray? districtsArray;
        var districts = saveObject.TryGetSaveArray("districts", out districtsArray) && districtsArray != null
            ? districtsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetString("value", out var value) ? value : string.Empty)
                .ToImmutableArray()
            : ImmutableArray<string>.Empty;

        SaveArray? moonsArray;
        var moons = saveObject.TryGetSaveArray("moons", out moonsArray) && moonsArray != null
            ? moonsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetLong("value", out var value) ? value : 0)
                .ToImmutableArray()
            : ImmutableArray<long>.Empty;

        bool isMoon = false;
        saveObject.TryGetBool("is_moon", out isMoon);

        bool hasRing = false;
        saveObject.TryGetBool("has_ring", out hasRing);

        long? moonOf = null;
        if (saveObject.TryGetLong("moon_of", out var moonOfValue))
        {
            moonOf = moonOfValue;
        }

        DateOnly? colonizeDate = null;
        string colonizeDateStr;
        if (saveObject.TryGetString("colonize_date", out colonizeDateStr))
        {
            colonizeDate = DateOnly.Parse(colonizeDateStr);
        }

        return new Planet
        {
            Id = id,
            Name = name,
            Class = planetClass,
            Size = size,
            Owner = owner,
            OriginalOwner = originalOwner,
            Controller = controller,
            Position = position,
            Deposits = deposits,
            Pops = pops,
            Buildings = buildings,
            Districts = districts,
            Moons = moons,
            IsMoon = isMoon,
            HasRing = hasRing,
            MoonOf = moonOf,
            ColonizeDate = colonizeDate
        };
    }
}
