

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class ShipSection
{
    [SaveScalar("design")]
    public required string Design { get; set; }

    [SaveScalar("slot")] 
    public required string Slot { get; set; }
}