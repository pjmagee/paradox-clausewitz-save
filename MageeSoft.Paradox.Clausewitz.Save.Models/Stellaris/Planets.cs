using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

public class Planets
{
    [SaveArray("planet")]
    public Dictionary<long, Planet> Values { get; set; } = new();
}