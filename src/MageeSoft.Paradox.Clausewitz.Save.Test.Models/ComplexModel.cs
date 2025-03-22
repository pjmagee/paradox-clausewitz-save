using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ComplexModel
{
    [SaveScalar("name")]
    public string Name { get; set; } = string.Empty;

    [SaveScalar("capital")]
    public int Capital { get; set; }

    [SaveObject("resources")]
    public NestedModel Resources { get; set; } = new();

    [SaveArray("planets")]
    public List<NestedModel> Planets { get; set; } = new();

    [SaveArray("values")]
    public float[] Values { get; set; } = [];

    [SaveArray("tags")]
    public string[] Tags { get; set; } = [];

    [SaveScalar("enabled")]
    public bool Enabled { get; set; }

    [SaveScalar("disabled")]
    public bool Disabled { get; set; }

    [SaveScalar("start_date")]
    public DateOnly StartDate { get; set; }

    [SaveObject("nested")]
    public NestedModel Nested { get; set; } = new();
}