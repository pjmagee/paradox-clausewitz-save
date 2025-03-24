using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class Exhibit
{
    [SaveScalar("exhibit_state")]
    public string? State { get; set; } = "";

    [SaveObject("specimen")]
    public ExhibitSpecimen? Specimen { get; set; }

    [SaveScalar("owner")]
    public string? Owner { get; set; } = "";

    [SaveScalar("date_added")]
    public DateOnly? DateAdded { get; set; }
}