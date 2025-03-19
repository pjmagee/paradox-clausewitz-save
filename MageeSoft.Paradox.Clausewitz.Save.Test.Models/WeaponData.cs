using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class WeaponData
{
    [SaveScalar("index")]
    public int Index { get; set; }

    [SaveScalar("template")]
    public string Template { get; set; } = "";

    [SaveScalar("component_slot")]
    public string ComponentSlot { get; set; } = "";
}