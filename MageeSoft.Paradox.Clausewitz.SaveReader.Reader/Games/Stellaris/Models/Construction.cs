using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a construction in the game state.
/// </summary>
public record Construction
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the planet.
    /// </summary>
    public required int Planet { get; init; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public required float Progress { get; init; }

    /// <summary>
    /// Gets or sets whether the construction is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public ImmutableDictionary<string, float> Resources { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Default instance of Construction.
    /// </summary>
    public static Construction Default => new()
    {
        Type = string.Empty,
        Planet = 0,
        Progress = 0f,
        IsActive = false,
        Resources = ImmutableDictionary<string, float>.Empty
    };

    /// <summary>
    /// Loads all constructions from a game state root object.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of constructions.</returns>
    public static ImmutableArray<Construction> Load(SaveObject root)
    {
        SaveObject? constructionsObj;
        if (!root.TryGetSaveObject("constructions", out constructionsObj))
        {
            return ImmutableArray<Construction>.Empty;
        }

        var constructions = constructionsObj.Properties
            .Select(kvp => kvp.Value)
            .OfType<SaveObject>()
            .Select(LoadSingle)
            .Where(x => x != null)
            .ToImmutableArray();

        return constructions!;
    }

    /// <summary>
    /// Loads a single construction from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the construction data.</param>
    /// <returns>A new Construction instance.</returns>
    private static Construction? LoadSingle(SaveObject obj)
    {
        string type;
        int planet;
        float progress;
        bool isActive;

        if (!obj.TryGetString("type", out type) ||
            !obj.TryGetInt("planet", out planet) ||
            !obj.TryGetFloat("progress", out progress) ||
            !obj.TryGetBool("is_active", out isActive))
        {
            return null;
        }

        SaveObject? resourcesObj;
        var resources = obj.TryGetSaveObject("resources", out resourcesObj) && resourcesObj != null
            ? resourcesObj.Properties
                .Where(kvp => kvp.Value is Scalar<float>)
                .ToImmutableDictionary(
                    kvp => kvp.Key,
                    kvp => ((Scalar<float>)kvp.Value).Value)
            : ImmutableDictionary<string, float>.Empty;

        return new Construction
        {
            Type = type,
            Planet = planet,
            Progress = progress,
            IsActive = isActive,
            Resources = resources
        };
    }
} 






