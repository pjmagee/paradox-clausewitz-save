using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class SimpleTestModel
{
    [SaveScalar("int_value")]
    public int? IntValue { get; set; }

    [SaveScalar("string_value")]
    public string? StringValue { get; set; }

    [SaveScalar("bool_value")]
    public bool? BoolValue { get; set; }

    [SaveScalar("float_value")]
    public float? FloatValue { get; set; }

    [SaveScalar("long_value")]
    public long? LongValue { get; set; }

    [SaveScalar("date_value")]
    public DateOnly? DateValue { get; set; }

    [SaveScalar("guid_value")]
    public Guid? GuidValue { get; set; }
    
    [SaveArray("array_value")]
    public int[]? ArrayValue { get; set; }
}