using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class GameState
{
    [SaveIndexedDictionary("country")]
    public Dictionary<int, Country>? Countries { get;set; } = new();

    [SaveIndexedDictionary("fleet")]
    public Dictionary<int, Fleet>? Fleets { get;set; } = new();

    [SaveObject("galaxy")]
    public Galaxy? Galaxy { get;set; } = new();

    [SaveIndexedDictionary("pop")]
    public Dictionary<int, Pop>? Pops { get;set; } = new();

    [SaveObject("planets")]
    public Planets? Planets { get;set; }

    [SaveIndexedDictionary("ships")]
    public Dictionary<int, Ship>? Ships { get; set; }
    
    [SaveIndexedDictionary("sectors")]
    public Dictionary<int, object>? Sectors { get; set; }
    
    [SaveIndexedDictionary("megastructures")]
    public Dictionary<int, Megastructure>? Megastructures { get; set; }
    
    [SaveIndexedDictionary("armies")]
    public Dictionary<int, object>? Armies { get; set; }
    
    [SaveObject("achievements")]
    public Achievements? Achievements { get;set; } = new();
} 