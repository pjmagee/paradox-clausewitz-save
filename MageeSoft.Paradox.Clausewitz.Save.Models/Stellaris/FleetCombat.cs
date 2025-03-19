namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents fleet combat in the game state.
/// </summary>
[SaveModel]
public partial class FleetCombat
{
    /// <summary>
    /// Gets or sets the fleet ID.
    /// </summary>
    public long Fleet { get; set; }

    /// <summary>
    /// Gets or sets the combat stats.
    /// </summary>
    public CombatStats CombatStats { get; set; }

    /// <summary>
    /// Gets or sets the fleet stats.
    /// </summary>
    public FleetStats FleetStats { get; set; }
}






