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
    public CombatStats CombatStats { get; set; } = new();

    /// <summary>
    /// Gets or sets the fleet stats.
    /// </summary>
    public FleetStats FleetStats { get; set; } = new();

    /// <summary>
    /// Loads fleet combat data from a SaveObject.
    /// </summary>
    /// <param name="element">The SaveObject containing the fleet combat data.</param>
    /// <returns>A new FleetCombat instance.</returns>
    public static FleetCombat Load(SaveObject element)
    {
        var fleetCombat = new FleetCombat();

        foreach (var property in element.Properties)
        {
            switch (property.Key)
            {
                case "fleet" when property.Value is Scalar<long> fleetScalar:
                    fleetCombat.Fleet = fleetScalar.Value;
                    break;
                case "combat_stats" when property.Value is SaveObject combatStatsObj:
                    fleetCombat.CombatStats = CombatStats.Load(combatStatsObj);
                    break;
                case "fleet_stats" when property.Value is SaveObject fleetStatsObj:
                    fleetCombat.FleetStats = FleetStats.Load(fleetStatsObj);
                    break;
            }
        }

        return fleetCombat;
    }
}