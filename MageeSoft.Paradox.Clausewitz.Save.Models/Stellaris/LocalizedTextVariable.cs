using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;


public class LocalizedTextVariable
{
    [SaveScalar("key")]
    public required string Key { get; set; }
    
    [SaveObject("value")]
    public required LocalizedTextValue Value { get; set; }
}