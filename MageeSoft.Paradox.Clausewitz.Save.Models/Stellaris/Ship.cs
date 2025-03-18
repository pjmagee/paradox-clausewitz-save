using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a ship in the game state.
/// </summary>
public class Ship
{
    [SaveObject("name")]
    public required LocalizedText Name { get; init; }
    
    public required long DesignId { get; init; }

    [SaveScalar("fleet")]
    public required long Fleet { get; init; }
    
    [SaveScalar("reserve")]
    public int Reserved { get; set; }
    
    [SaveScalar("ship_design")]
    public int ShipDesign { get; set; }
    
    [SaveScalar("is_original_design")]
    public bool IsOriginalDesign { get; set; }
    
    [SaveScalar("design_upgrade")]
    public int DesignUpgrade { get; set; }
    
    [SaveScalar("graphical_culture")]
    public string GraphicalCulture { get; set; }
    
    [SaveArray("section")]
    public ShipSection[] Section { get; set; }
}