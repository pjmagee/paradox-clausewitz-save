using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a megastructure in the game state.
/// </summary>
public record Megastructure
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get; init; }

    /// <summary>
    /// Gets or sets the stage.
    /// </summary>
    public string Stage { get; init; }

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public Coordinate Coordinate { get; init; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public float Progress { get; init; }

    /// <summary>
    /// Gets or sets whether the megastructure is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Creates a new instance of Megastructure with default values.
    /// </summary>
    public Megastructure()
    {
        Id = 0;
        Type = string.Empty;
        Stage = string.Empty;
        Coordinate = Models.Coordinate.Default;
        Progress = 0f;
        IsActive = false;
    }

    /// <summary>
    /// Creates a new instance of Megastructure with specified values.
    /// </summary>
    public Megastructure(long id, string type, string stage, Coordinate coordinate, float progress, bool isActive)
    {
        Id = id;
        Type = type ?? string.Empty;
        Stage = stage ?? string.Empty;
        Coordinate = coordinate ?? Coordinate.Default;
        Progress = progress;
        IsActive = isActive;
    }

    /// <summary>
    /// Loads all megastructures from a game state root object.
    /// </summary>
    /// <param name="rootObject">The game state root object.</param>
    /// <returns>An immutable array of megastructures.</returns>
    public static ImmutableArray<Megastructure> Load(SaveObject rootObject)
    {
        SaveObject? megastructures;

        if (!rootObject.TryGetSaveObject("megastructures", out megastructures) || megastructures == null)
        {
            return ImmutableArray<Megastructure>.Empty;
        }

        // Needs reviewing how this works with gamestate-megastructures file

        // var items = megastructures.Elements()
        //     .OfType<SaveObject>()
        //     .Select(LoadSingle)
        //     .Where(x => x != null)
        //     .ToImmutableArray();

        // return megastructures!;

        return ImmutableArray<Megastructure>.Empty;
    }

    /// <summary>
    /// Loads a single megastructure from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the megastructure data.</param>
    /// <returns>A new Megastructure instance.</returns>
    private static Megastructure? LoadSingle(SaveObject saveObject)
    {
        long id;
        string type;
        string stage;
        float progress;
        bool isActive;
        Coordinate? coordinate;

        // there is no 'id' field, as its the key in the key=value properties in the megastructures object

        if (!saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetString("stage", out stage) ||
            !saveObject.TryGetFloat("progress", out progress) ||
            !saveObject.TryGetBool("is_active", out isActive))
        {
            return null;
        }

        SaveObject? coordinateObj;
        if (!saveObject.TryGetSaveObject("coordinate", out coordinateObj) || coordinateObj == null)
        {
            return null;
        }

        coordinate = Coordinate.Load(coordinateObj);
        if (coordinate == null)
        {
            return null;
        }

        return new Megastructure(id, type, stage, coordinate, progress, isActive);
    }
}