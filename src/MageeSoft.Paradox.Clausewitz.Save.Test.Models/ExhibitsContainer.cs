using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ExhibitsContainer
{
    [SaveIndexedDictionary("exhibits")]
    public ImmutableDictionary<int, Exhibit> Exhibits { get; set; }
}