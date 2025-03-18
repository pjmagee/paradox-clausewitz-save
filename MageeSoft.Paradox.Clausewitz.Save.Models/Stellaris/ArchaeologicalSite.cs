using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

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
}