using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a fleet in the game state.
/// </summary>
public class Fleet
{
    [SaveObject("name")]
    public LocalizedText Name { get; init; }
    
    [SaveArray("ships")]
    public int[] Ships { get; init; }

    [SaveObject("combat")]
    public Combat Combat { get; init; }

    [SaveObject("fleet_stats")]
    public FleetStats FleetStats { get; init; }

    [SaveScalar("station")]
    public required bool IsStation { get; init; }

    [SaveScalar("ground_support_stance")]
    public required string GroundSupportStance { get; init; }

    [SaveScalar("space_fauna_growth_stance")]
    public required string SpaceFaunaGrowthStance { get; init; }

    [SaveObject("mia_from")]
    public required Position MiaFrom { get; init; }

    [SaveObject("movement_manager")]
    public required FleetMovementManager MovementManager { get; init; }

    [SaveScalar("hit_points")]
    public required float HitPoints { get; init; }

    [SaveScalar("military_power")]
    public required float MilitaryPower { get; init; }
    
    [SaveScalar("diplomacy_weight")]
    public required float DiplomacyWeight { get; init; }

    [SaveScalar("cached_killed_ships")]
    public required int CachedKilledShips { get; init; }

    [SaveScalar("cached_disabled_ships")]
    public required int CachedDisabledShips { get; init; }

    [SaveScalar("cached_disengaged_ships")]
    public required int CachedDisengagedShips { get; init; }

    [SaveScalar("cached_combined_removed_ships")]
    public required int CachedCombinedRemovedShips { get; init; }

    [SaveScalar("can_take_orders")]
    public required bool CanTakeOrders { get; init; }
}






