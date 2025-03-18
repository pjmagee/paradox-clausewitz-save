using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

public class ShipSection
{
    [SaveScalar("design")]
    public required string Design { get; set; }

    [SaveScalar("slot")] 
    public required string Slot { get; set; }
}