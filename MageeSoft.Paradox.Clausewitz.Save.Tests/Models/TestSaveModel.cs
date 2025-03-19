using MageeSoft.Paradox.Clausewitz.Save.Models;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Models;

[SaveModel]
public partial class TestSaveModel
{
    [SaveScalar("int_value")]
    public int IntValue { get; set; }

    [SaveScalar("string_value")]
    public string StringValue { get; set; } = string.Empty;

    [SaveScalar("date_value")]
    public DateOnly DateValue { get; set; }

    [SaveArray("array_value")]
    public int[] ArrayValue { get; set; } = Array.Empty<int>();

    [SaveIndexedDictionary("dict_value")]
    public ImmutableDictionary<int, string> DictValue { get; set; } = ImmutableDictionary<int, string>.Empty;
} 