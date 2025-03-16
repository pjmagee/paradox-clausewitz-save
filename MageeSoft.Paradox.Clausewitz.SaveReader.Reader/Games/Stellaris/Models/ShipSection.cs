using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public class ShipSection
{
    [SaveScalar("design")]
    public required string Design { get; set; }

    [SaveScalar("slot")] 
    public required string Slot { get; set; }
}