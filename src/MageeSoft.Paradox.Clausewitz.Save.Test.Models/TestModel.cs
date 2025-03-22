using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class TestModel
{
    [SaveScalar("name")]
    public string Name { get; set; } = string.Empty;

    [SaveScalar("capital")]
    public int Capital { get; set; }

    [SaveScalar("start_date")]
    public DateOnly StartDate { get; set; }

    [SaveScalar("ironman")]
    public bool Ironman { get; set; }

    [SaveArray("achievement")]
    public int[] Achievements { get; set; } = Array.Empty<int>();

    [SaveScalar("id")]
    public Guid Id { get; set; }
}