using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class TestModelForSourceGen
{
    [SaveScalar("int_val")]
    public int IntValue { get; set; }
    
    [SaveScalar("string_val")]
    public string StringValue { get; set; } = string.Empty;
    
    [SaveArray("int_array")]
    public int[] IntArray { get; set; } = Array.Empty<int>();
    
    [SaveArray("immutable_list")]
    public ImmutableList<int> ImmutableIntList { get; set; } = ImmutableList<int>.Empty;
    
    [SaveIndexedDictionary("int_dict")]
    public Dictionary<int, string> IntStringDict { get; set; } = new();
    
    [SaveIndexedDictionary("immutable_dict")]
    public ImmutableDictionary<int, string> ImmutableIntStringDict { get; set; } = ImmutableDictionary<int, string>.Empty;
    
    [SaveArray("nested_array")]
    public NestedModel[] NestedArray { get; set; } = Array.Empty<NestedModel>();
    
    [SaveScalar("date_value")]
    public DateOnly DateValue { get; set; }
    
    [SaveArray("array_value")]
    public int[] ArrayValue { get; set; } = Array.Empty<int>();
}