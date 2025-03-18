using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

public class LocalizedTextValue
{
    [SaveScalar("key")]
    public string Key { get; init; }
}