using MageeSoft.Paradox.Clausewitz.Save.Models;

namespace MageeSoft.Paradox.Clausewitz.Save.Test.Models;

[SaveModel]
public partial class ExhibitSpecimen
{
    [SaveScalar("id")]
    public string? Id { get; set; }

    [SaveScalar("origin")]
    public string? Origin { get; set; }
}