using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class Achievements
{    
    [SaveArray("achievement")]
    public List<int>? Values { get; set; }
} 






