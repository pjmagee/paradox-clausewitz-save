using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

public class Achievements
{    
    [SaveArray("achievement")]
    public ImmutableList<int> Values { get; init; }
} 






