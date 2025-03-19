

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a ship in the game state.
/// </summary>
[SaveModel]
public partial class Ship
{
    [SaveObject("name")]
    public required LocalizedText Name { get;set; }
    
    public required long DesignId { get;set; }

    [SaveScalar("fleet")]
    public required long Fleet { get;set; }
    
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