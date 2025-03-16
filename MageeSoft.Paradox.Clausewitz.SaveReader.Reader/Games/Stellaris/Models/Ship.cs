using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a ship in the game state.
/// </summary>
public class Ship
{
    [SaveProperty("name")]
    public required LocalizedText Name { get; init; }
    
    public required long DesignId { get; init; }

    [SaveProperty("fleet")]
    public required long Fleet { get; init; }
    
    [SaveProperty("reserve")]
    public int Reserved { get; set; }
    
    [SaveProperty("ship_design")]
    public int ShipDesign { get; set; }
    
    [SaveProperty("is_original_design")]
    public bool IsOriginalDesign { get; set; }
    
    [SaveProperty("design_upgrade")]
    public int DesignUpgrade { get; set; }
    
    [SaveProperty("graphical_culture")]
    public string GraphicalCulture { get; set; }
    
    [SaveProperty("section")]
    public ShipSection[] Section { get; set; }
}

public class ShipSection
{
    [SaveProperty("design")]
    public required string Design { get; set; }

    [SaveProperty("slot")] public required string Slot { get; set; }
}




