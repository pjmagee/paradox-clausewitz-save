using System.Collections.Immutable;


namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a construction in the game state.
/// </summary>
[SaveModel]
public partial class Construction
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the planet.
    /// </summary>
    public int Planet { get;set; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public float Progress { get;set; }

    /// <summary>
    /// Gets or sets whether the construction is active.
    /// </summary>
    public bool IsActive { get;set; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public ImmutableDictionary<string, float> Resources { get;set; }
} 






