using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class TestSaveModel
{
    [SaveScalar("int_value")]
    public int? IntValue { get; set; }

    [SaveScalar("string_value")]
    public string? StringValue { get; set; } = string.Empty;

    [SaveScalar("date_value")]
    public DateOnly? DateValue { get; set; }

    [SaveArray("array_value")]
    public int[]? ArrayValue { get; set; }

    [SaveIndexedDictionary("dict_value")]
    public Dictionary<int, string>? DictValue { get; set; }
} 