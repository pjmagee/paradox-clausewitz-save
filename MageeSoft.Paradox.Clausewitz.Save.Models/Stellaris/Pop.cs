using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a pop in the game state.
/// </summary>
public class Pop
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the species id
    /// </summary>
    [SaveScalar("species")]
    public required int Species { get; init; }

    /// <summary>
    /// Gets or sets the faction.
    /// </summary>
    public required PopFaction Faction { get; init; }

    /// <summary>
    /// Gets or sets the happiness.
    /// </summary>
    public required float Happiness { get; init; }

    /// <summary>
    /// Gets or sets the power.
    /// </summary>
    public required float Power { get; init; }
}