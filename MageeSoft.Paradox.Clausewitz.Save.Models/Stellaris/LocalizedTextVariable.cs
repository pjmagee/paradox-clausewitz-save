namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class LocalizedTextVariable
{
    [SaveScalar("key")]
    public required string Key { get; set; }
    
    [SaveObject("value")]
    public required LocalizedTextValue Value { get; set; }
}