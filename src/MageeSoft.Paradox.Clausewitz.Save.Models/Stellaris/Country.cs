namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class Country
{
    [SaveScalar("name")]
    public string Name { get;set; } = string.Empty;
} 