using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;
using MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

namespace MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris;

public class GameState
{
    [SaveIndexedDictionary("fleet")]
    public Dictionary<int, Fleet> Fleets { get; set; }
    
    [SaveIndexedDictionary("ships")]
    public ImmutableDictionary<int, Ship> Ships { get; set; }
    
    [SaveObject("planets")]
    public Planets Planets { get; set; }
    
    [SaveIndexedDictionary("sectors")]
    public Dictionary<int, Sector> Sectors { get; set; }
    
    // [SaveProperty("species_db")]
    // public Dictionary<int, Species> Species { get; set; }
    
    [SaveIndexedDictionary("armies")]
    public Dictionary<int, Army> Armies { get; set; }
   
    [SaveObject("achievements")]
    public Achievements Achievements { get; set; }
}