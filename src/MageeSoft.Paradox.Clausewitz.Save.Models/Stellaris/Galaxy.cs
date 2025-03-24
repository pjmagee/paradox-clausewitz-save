namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class Galaxy
{
    [SaveScalar("shape")]
    public string? Shape { get;set; } = string.Empty;
} 