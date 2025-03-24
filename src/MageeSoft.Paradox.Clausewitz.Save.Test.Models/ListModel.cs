using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ListModel
{
    [SaveArray("values")]
    public List<int>? Values { get;set; }

    [SaveArray("strings")]
    public List<string>? Strings { get;set; }

    [SaveArray("nested")]
    public List<NestedModel?>? Nested { get;set; }
}