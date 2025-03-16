using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents fleet combat in the game state.
/// </summary>
public class FleetCombat
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






