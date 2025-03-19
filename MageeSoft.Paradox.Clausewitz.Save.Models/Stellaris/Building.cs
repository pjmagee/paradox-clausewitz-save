using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;


[SaveModel]
public partial class Building
{
    public required long Id { get;set; }
    public required string Type { get;set; }
    public required int RuinTime { get;set; }
} 






