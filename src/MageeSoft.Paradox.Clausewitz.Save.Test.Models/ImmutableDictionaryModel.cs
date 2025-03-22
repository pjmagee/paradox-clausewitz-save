using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ImmutableDictionaryModel
{
    [SaveIndexedDictionary("resources")]
    public ImmutableDictionary<int, NestedModel?> Resources { get;set; } = ImmutableDictionary<int, NestedModel?>.Empty;

    [SaveIndexedDictionary("scores")]
    public ImmutableDictionary<int, float> Scores { get;set; } = ImmutableDictionary<int, float>.Empty;
}