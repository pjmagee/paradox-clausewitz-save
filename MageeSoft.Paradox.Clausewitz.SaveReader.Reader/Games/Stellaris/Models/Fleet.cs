using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a fleet in the game state.
/// </summary>
public class Fleet
{
    [SaveProperty("name")]
    public LocalizedText Name { get; init; }
    
    [SaveProperty("ships")]
    public int[] Ships { get; init; }

    [SaveProperty("combat")]
    public Combat Combat { get; init; }

    [SaveProperty("fleet_stats")]
    public FleetStats FleetStats { get; init; }

    [SaveProperty("station")]
    public required bool IsStation { get; init; }

    [SaveProperty("ground_support_stance")]
    public required string GroundSupportStance { get; init; }

    [SaveProperty("space_fauna_growth_stance")]
    public required string SpaceFaunaGrowthStance { get; init; }

    [SaveProperty("mia_from")]
    public required Position MiaFrom { get; init; }

    [SaveProperty("movement_manager")]
    public required FleetMovementManager MovementManager { get; init; }

    [SaveProperty("hit_points")]
    public required float HitPoints { get; init; }

    [SaveProperty("military_power")]
    public required float MilitaryPower { get; init; }
    
    [SaveProperty("diplomacy_weight")]
    public required float DiplomacyWeight { get; init; }

    [SaveProperty("cached_killed_ships")]
    public required int CachedKilledShips { get; init; }

    [SaveProperty("cached_disabled_ships")]
    public required int CachedDisabledShips { get; init; }

    [SaveProperty("cached_disengaged_ships")]
    public required int CachedDisengagedShips { get; init; }

    [SaveProperty("cached_combined_removed_ships")]
    public required int CachedCombinedRemovedShips { get; init; }

    [SaveProperty("can_take_orders")]
    public required bool CanTakeOrders { get; init; }
}






