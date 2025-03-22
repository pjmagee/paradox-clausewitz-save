using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class SectionData
{
    [SaveScalar("design")]
    public string Design { get; set; } = "";

    [SaveScalar("slot")]
    public string Slot { get; set; } = "";

    [SaveArray("weapon")]
    public ImmutableList<WeaponData> Weapons { get; set; }
}