using MageeSoft.Paradox.Clausewitz.SaveReader.Model.Attributes;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

public class Planets
{
    [SaveProperty("planet")]
    public Dictionary<long, Planet> Values { get; set; } = new();
}

public class GameState
{
    [SaveProperty("fleet")]
    public Dictionary<int, Fleet> Fleets { get; set; }
    
    [SaveProperty("ships")]
    public Dictionary<int, Ship> Ships { get; set; }
    
    [SaveProperty("planets")]
    public Planets Planets { get; set; }
    
    [SaveProperty("sectors")]
    public Dictionary<int, Sector> Sectors { get; set; }
    
    // [SaveProperty("species_db")]
    // public Dictionary<int, Species> Species { get; set; }
    
    [SaveProperty("armies")]
    public Dictionary<int, Army> Armies { get; set; }
   
    [SaveProperty("achievements")]
    public Achievements Achievements { get; set; }
}