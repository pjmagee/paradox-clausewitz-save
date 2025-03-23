namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class LocalizedTextVariable
{
    [SaveScalar("key")]
    public string Key { get; set; }
    
    [SaveObject("value")]
    public LocalizedTextValue Value { get; set; }
}