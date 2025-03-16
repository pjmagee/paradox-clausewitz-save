using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

public class Planets
{
    [SaveArray("planet")]
    public Dictionary<long, Planet> Values { get; set; } = new();
}

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