

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a collection of planets in the game state.
/// </summary>
[SaveModel]
public partial class Planets
{
    [SaveIndexedDictionary("planet")]
    public Dictionary<long, Planet>? Values { get; set; } = new();
}