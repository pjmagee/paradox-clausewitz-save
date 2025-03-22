using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ImmutableListModel
{
    [SaveArray("values")]
    public ImmutableList<int> Values { get;set; }

    [SaveArray("strings")]
    public ImmutableList<string> Strings { get;set; }

    [SaveArray("nested")]
    public ImmutableList<NestedModel?> Nested { get;set; }
}