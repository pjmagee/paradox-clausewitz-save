using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a first contact scope in the game state.
/// </summary>
public record FirstContactScope
{
    /// <summary>
    /// Gets the default instance of FirstContactScope.
    /// </summary>
    public static FirstContactScope Default { get; } = new()
    {
        Type = string.Empty,
        Id = 0,
        OpenerId = 0,
        RandomAllowed = false,
        Random = 0f,
        Root = 0,
        From = 0,
        Systems = ImmutableArray<long>.Empty,
        Planets = ImmutableArray<long>.Empty,
        Fleets = ImmutableArray<long>.Empty
    };

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the opener ID.
    /// </summary>
    public required long OpenerId { get; init; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public required bool RandomAllowed { get; init; }

    /// <summary>
    /// Gets or sets the random value.
    /// </summary>
    public required float Random { get; init; }

    /// <summary>
    /// Gets or sets the root.
    /// </summary>
    public required long Root { get; init; }

    /// <summary>
    /// Gets or sets the from value.
    /// </summary>
    public required long From { get; init; }

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
    /// Creates a new instance of FirstContactScope with default values.
    /// </summary>
    public FirstContactScope()
    {
        Type = string.Empty;
        Id = 0;
        OpenerId = 0;
        RandomAllowed = false;
        Random = 0f;
        Root = 0;
        From = 0;
        Systems = ImmutableArray<long>.Empty;
        Planets = ImmutableArray<long>.Empty;
        Fleets = ImmutableArray<long>.Empty;
    }

    /// <summary>
    /// Creates a new instance of FirstContactScope with specified values.
    /// </summary>
    public FirstContactScope(string type, long id, long openerId, bool randomAllowed, float random, long root, long from, ImmutableArray<long> systems, ImmutableArray<long> planets, ImmutableArray<long> fleets)
    {
        Type = type ?? string.Empty;
        Id = id;
        OpenerId = openerId;
        RandomAllowed = randomAllowed;
        Random = random;
        Root = root;
        From = from;
        Systems = systems.IsDefault ? ImmutableArray<long>.Empty : systems;
        Planets = planets.IsDefault ? ImmutableArray<long>.Empty : planets;
        Fleets = fleets.IsDefault ? ImmutableArray<long>.Empty : fleets;
    }

    /// <summary>
    /// Loads a first contact scope from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the first contact scope data.</param>
    /// <returns>A new FirstContactScope instance.</returns>
    public static FirstContactScope? Load(SaveObject saveObject)
    {
        if (!saveObject.TryGetString("type", out var type) ||
            !saveObject.TryGetLong("id", out var id) ||
            !saveObject.TryGetLong("opener_id", out var openerId) ||
            !saveObject.TryGetBool("random_allowed", out var randomAllowed) ||
            !saveObject.TryGetFloat("random", out var random) ||
            !saveObject.TryGetLong("root", out var root) ||
            !saveObject.TryGetLong("from", out var from))
        {
            return null;
        }

        var systems = saveObject.TryGetSaveArray("systems", out var systemsArray) && systemsArray != null
            ? LoadLongArray(systemsArray)
            : ImmutableArray<long>.Empty;

        var planets = saveObject.TryGetSaveArray("planets", out var planetsArray) && planetsArray != null
            ? LoadLongArray(planetsArray)
            : ImmutableArray<long>.Empty;

        var fleets = saveObject.TryGetSaveArray("fleets", out var fleetsArray) && fleetsArray != null
            ? LoadLongArray(fleetsArray)
            : ImmutableArray<long>.Empty;

        return new FirstContactScope
        {
            Type = type,
            Id = id,
            OpenerId = openerId,
            RandomAllowed = randomAllowed,
            Random = random,
            Root = root,
            From = from,
            Systems = systems,
            Planets = planets,
            Fleets = fleets
        };
    }

    /// <summary>
    /// Loads an array of longs from a SaveArray.
    /// </summary>
    /// <param name="array">The SaveArray containing long values.</param>
    /// <returns>An immutable array of longs.</returns>
    private static ImmutableArray<long> LoadLongArray(SaveArray array)
    {
        var builder = ImmutableArray.CreateBuilder<long>();

        foreach (var item in array.Elements())
        {
            if (item is SaveObject obj && obj.TryGetLong("value", out var value))
            {
                builder.Add(value);
            }
        }

        return builder.ToImmutable();
    }
}






