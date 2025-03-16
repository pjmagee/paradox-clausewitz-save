using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an archaeological site in the game state.
/// </summary>
public record ArchaeologicalSite
{
    /// <summary>
    /// Gets or sets the site ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the planet ID where the site is located.
    /// </summary>
    public required long Planet { get; init; }

    /// <summary>
    /// Gets or sets the owner ID.
    /// </summary>
    public required long Owner { get; init; }

    /// <summary>
    /// Gets or sets the type of the archaeological site.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the current stage of the excavation.
    /// </summary>
    public required int Stage { get; init; }

    /// <summary>
    /// Gets or sets the current progress of the excavation.
    /// </summary>
    public required int Progress { get; init; }

    /// <summary>
    /// Gets or sets the total progress required for the current stage.
    /// </summary>
    public required int TotalProgress { get; init; }

    /// <summary>
    /// Gets or sets the total number of stages.
    /// </summary>
    public required int TotalStages { get; init; }

    /// <summary>
    /// Gets or sets the total number of clues found.
    /// </summary>
    public required int TotalClues { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the site is being excavated.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the current chapter of the excavation.
    /// </summary>
    public required int Chapter { get; init; }

    /// <summary>
    /// Gets or sets the researcher ID.
    /// </summary>
    public required long Researcher { get; init; }

    /// <summary>
    /// Default instance of ArchaeologicalSite.
    /// </summary>
    public static ArchaeologicalSite Default => new()
    {
        Id = 0,
        Planet = 0,
        Owner = 0,
        Type = string.Empty,
        Stage = 0,
        Progress = 0,
        TotalProgress = 0,
        TotalStages = 0,
        TotalClues = 0,
        IsActive = false,
        Chapter = 0,
        Researcher = 0
    };

    /// <summary>
    /// Loads all archaeological sites from the game state.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of archaeological sites.</returns>
    public static ImmutableArray<ArchaeologicalSite> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<ArchaeologicalSite>();

        if (!root.TryGetSaveObject("archaeological_sites", out var sitesObj))
        {
            return builder.ToImmutable();
        }

        foreach (var (key, value) in sitesObj.Properties)
        {
            if (long.TryParse(key, out var siteId) && value is SaveObject obj)
            {
                var site = LoadSingle(obj);
                if (site != null)
                {
                    builder.Add(site with { Id = siteId });
                }
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Loads a single archaeological site from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the site data.</param>
    /// <returns>A new ArchaeologicalSite instance if successful, null otherwise.</returns>
    private static ArchaeologicalSite? LoadSingle(SaveObject obj)
    {
        if (!obj.TryGetLong("planet", out var planet) ||
            !obj.TryGetLong("owner", out var owner) ||
            !obj.TryGetString("type", out var type) ||
            !obj.TryGetInt("stage", out var stage) ||
            !obj.TryGetInt("progress", out var progress) ||
            !obj.TryGetInt("total_progress", out var totalProgress) ||
            !obj.TryGetInt("total_stages", out var totalStages) ||
            !obj.TryGetInt("total_clues", out var totalClues) ||
            !obj.TryGetBool("active", out var isActive) ||
            !obj.TryGetInt("chapter", out var chapter) ||
            !obj.TryGetLong("researcher", out var researcher))
        {
            return null;
        }

        return new ArchaeologicalSite
        {
            Id = 0, // Will be set by the caller
            Planet = planet,
            Owner = owner,
            Type = type,
            Stage = stage,
            Progress = progress,
            TotalProgress = totalProgress,
            TotalStages = totalStages,
            TotalClues = totalClues,
            IsActive = isActive,
            Chapter = chapter,
            Researcher = researcher
        };
    }
}