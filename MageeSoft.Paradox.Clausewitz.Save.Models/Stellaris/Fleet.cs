namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a fleet in the game state.
/// </summary>  
[SaveModel]
public partial class Fleet
{
    [SaveObject("name")]
    public LocalizedText Name { get;set; }
    
    [SaveArray("ships")]
    public int[] Ships { get;set; }

    [SaveObject("combat")]
    public Combat Combat { get;set; }

    [SaveObject("fleet_stats")]
    public FleetStats FleetStats { get;set; }

    [SaveScalar("station")]
    public bool IsStation { get;set; }

    [SaveScalar("ground_support_stance")]
    public string GroundSupportStance { get;set; }

    [SaveScalar("space_fauna_growth_stance")]
    public string SpaceFaunaGrowthStance { get;set; }

    [SaveObject("mia_from")]
    public Position MiaFrom { get;set; }

    [SaveObject("movement_manager")]
    public FleetMovementManager MovementManager { get;set; }

    [SaveScalar("hit_points")]
    public float HitPoints { get;set; }

    [SaveScalar("military_power")]
    public float MilitaryPower { get;set; }
    
    [SaveScalar("diplomacy_weight")]
    public float DiplomacyWeight { get;set; }

    [SaveScalar("cached_killed_ships")]
    public int CachedKilledShips { get;set; }

    [SaveScalar("cached_disabled_ships")]
    public int CachedDisabledShips { get;set; }

    [SaveScalar("cached_disengaged_ships")]
    public int CachedDisengagedShips { get;set; }

    [SaveScalar("cached_combined_removed_ships")]
    public int CachedCombinedRemovedShips { get;set; }

    [SaveScalar("can_take_orders")]
    public bool CanTakeOrders { get;set; }
}






