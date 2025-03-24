using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class RepeatedPropertyModel
{
    [SaveArray("section")]
    public List<SectionData?>? Sections { get; set; }
}