namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a pop in the game state.
/// </summary>
[SaveModel]
public partial class Pop
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the species id
    /// </summary>
    [SaveScalar("species")]
    public int Species { get;set; }

    /// <summary>
    /// Gets or sets the faction.
    /// </summary>
    public PopFaction Faction { get;set; }

    /// <summary>
    /// Gets or sets the happiness.
    /// </summary>
    public float Happiness { get;set; }

    /// <summary>
    /// Gets or sets the power.
    /// </summary>
    public float Power { get;set; }
}