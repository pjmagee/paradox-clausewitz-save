

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class ShipSection
{
    [SaveScalar("design")]
    public string Design { get; set; }

    [SaveScalar("slot")] 
    public string Slot { get; set; }
}