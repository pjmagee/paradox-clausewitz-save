using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a situation scope in the game state.
/// </summary>
public record SituationScope
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public required long Country { get; init; }

    /// <summary>
    /// Gets or sets the systems.
    /// </summary>
    public required ImmutableArray<long> Systems { get; init; }

    /// <summary>
    /// Gets or sets the planets.
    /// </summary>
    public required ImmutableArray<long> Planets { get; init; }

    /// <summary>
    /// Gets or sets the fleets.
    /// </summary>
    public required ImmutableArray<long> Fleets { get; init; }

    /// <summary>
    /// Default instance of SituationScope.
    /// </summary>
    public static SituationScope Default => new()
    {
        Type = string.Empty,
        Id = 0,
        Country = 0,
        Systems = ImmutableArray<long>.Empty,
        Planets = ImmutableArray<long>.Empty,
        Fleets = ImmutableArray<long>.Empty
    };

    /// <summary>
    /// Loads a situation scope from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the situation scope data.</param>
    /// <returns>A new SituationScope instance.</returns>
    public static SituationScope? Load(SaveObject saveObject)
    {
        string type;
        long id;
        long country;

        if (!saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetLong("country", out country))
        {
            return null;
        }

        SaveArray? systemsArray;
        var systems = saveObject.TryGetSaveArray("systems", out systemsArray) && systemsArray != null
            ? systemsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetLong("value", out var value) ? value : 0)
                .ToImmutableArray()
            : ImmutableArray<long>.Empty;

        SaveArray? planetsArray;
        var planets = saveObject.TryGetSaveArray("planets", out planetsArray) && planetsArray != null
            ? planetsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetLong("value", out var value) ? value : 0)
                .ToImmutableArray()
            : ImmutableArray<long>.Empty;

        SaveArray? fleetsArray;
        var fleets = saveObject.TryGetSaveArray("fleets", out fleetsArray) && fleetsArray != null
            ? fleetsArray.Elements()
                .OfType<SaveObject>()
                .Select(x => x.TryGetLong("value", out var value) ? value : 0)
                .ToImmutableArray()
            : ImmutableArray<long>.Empty;

        return new SituationScope
        {
            Type = type,
            Id = id,
            Country = country,
            Systems = systems,
            Planets = planets,
            Fleets = fleets
        };
    }
} 






