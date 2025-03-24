using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class TestModelForSourceGen
{
    [SaveScalar("int_val")]
    public int? IntValue { get; set; }
    
    [SaveScalar("string_val")]
    public string? StringValue { get; set; } = string.Empty;
    
    [SaveArray("int_array")]
    public int[]? IntArray { get; set; } = Array.Empty<int>();
    
    [SaveArray("immutable_list")]
    public List<int>? ImmutableIntList { get; set; }
    
    [SaveIndexedDictionary("int_dict")]
    public Dictionary<int, string>? IntStringDict { get; set; } = new();
    
    [SaveIndexedDictionary("immutable_dict")]
    public Dictionary<int, string>? ImmutableIntStringDict { get; set; }
    
    [SaveArray("nested_array")]
    public NestedModel[]? NestedArray { get; set; }
    
    [SaveScalar("date_value")]
    public DateOnly? DateValue { get; set; }
    
    [SaveArray("array_value")]
    public int[]? ArrayValue { get; set; }
}