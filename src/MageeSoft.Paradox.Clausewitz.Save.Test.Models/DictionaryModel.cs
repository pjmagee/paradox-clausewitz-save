using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class DictionaryModel
{
    [SaveIndexedDictionary("resources")]
    public Dictionary<int, NestedModel>? Resources { get;set; }

    [SaveIndexedDictionary("scores")]
    public Dictionary<int, float>? Scores { get;set; }
}