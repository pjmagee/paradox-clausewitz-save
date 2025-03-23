using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

[SaveModel]
public partial class GameState
{
    [SaveIndexedDictionary("country")]
    public Dictionary<int, Country> Countries { get;set; } = new();

    [SaveIndexedDictionary("fleet")]
    public Dictionary<int, Fleet> Fleets { get;set; } = new();

    [SaveObject("galaxy")]
    public Galaxy Galaxy { get;set; } = new();

    [SaveIndexedDictionary("pop")]
    public Dictionary<int, Pop> Pops { get;set; } = new();

    [SaveIndexedDictionary("planets")]
    public Dictionary<int, Planet> Planets { get;set; } = new();

    [SaveIndexedDictionary("ships")]
    public ImmutableDictionary<int, Ship> Ships { get; set; } = ImmutableDictionary<int, Ship>.Empty;
    
    [SaveIndexedDictionary("sectors")]
    public ImmutableDictionary<int, object> Sectors { get; set; } = ImmutableDictionary<int, object>.Empty;
    
    [SaveIndexedDictionary("megastructures")]
    public ImmutableDictionary<int, Megastructure> Megastructures { get; set; } = ImmutableDictionary<int, Megastructure>.Empty;
    
    [SaveIndexedDictionary("armies")]
    public ImmutableDictionary<int, object> Armies { get; set; } = ImmutableDictionary<int, object>.Empty;
    
    [SaveObject("achievements")]
    public Achievements Achievements { get;set; } = new();
} 