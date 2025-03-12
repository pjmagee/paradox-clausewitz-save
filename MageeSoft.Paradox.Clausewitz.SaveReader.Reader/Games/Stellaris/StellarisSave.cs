using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris;

/// <summary>
/// Provides a high-level API for accessing Stellaris save data.
/// </summary>
public class StellarisSave
{
    readonly GameSaveDocuments _documents;

    /// <summary>
    /// Gets the meta information.
    /// </summary>
    public SaveObject Meta { get; private set; }

    /// <summary>
    /// Gets the game state.
    /// </summary>
    public SaveObject GameState { get; private set; }

    /// <summary>
    /// Gets the game version.
    /// </summary>
    public string Version { get; private set; }

    /// <summary>
    /// Gets the game date.
    /// </summary>
    public string Date { get; private set; }

    /// <summary>
    /// Gets the player name.
    /// </summary>
    public string Player { get; private set; }

    /// <summary>
    /// Gets the galaxy name.
    /// </summary>
    public string GalaxyName { get; private set; }

    /// <summary>
    /// Gets the agreements.
    /// </summary>
    ImmutableArray<Agreement>? _agreements;
    public ImmutableArray<Agreement> Agreements => _agreements ??= Agreement.Load(GameState);

    /// <summary>
    /// Gets the fleets.
    /// </summary>
    ImmutableArray<Fleet>? _fleets;
    public ImmutableArray<Fleet> Fleets => _fleets ??= Fleet.Load(GameState);

    /// <summary>
    /// Gets the formations.
    /// </summary>
    ImmutableArray<Formation>? _formations;
    public ImmutableArray<Formation> Formations => _formations ??= Formation.Load(GameState);

    /// <summary>
    /// Gets the sectors.
    /// </summary>
    ImmutableArray<Sector>? _sectors;
    public ImmutableArray<Sector> Sectors => _sectors ??= Sector.Load(GameState).ToImmutableArray();

    /// <summary>
    /// Gets the ship designs.
    /// </summary>
    ImmutableArray<ShipDesign>? _shipDesigns;
    public ImmutableArray<ShipDesign> ShipDesigns => _shipDesigns ??= ShipDesign.Load(GameState).ToImmutableArray();

    /// <summary>
    /// Gets the species.
    /// </summary>
    ImmutableArray<Species>? _species;
    public ImmutableArray<Species> Species => _species ??= GetSpecies();

    /// <summary>
    /// Gets the armies.
    /// </summary>
    ImmutableArray<Army>? _armies;
    public ImmutableArray<Army> Armies => _armies ??= GetArmies();

    /// <summary>
    /// Gets the ambient objects.
    /// </summary>
    ImmutableArray<AmbientObject>? _ambientObjects;
    public ImmutableArray<AmbientObject> AmbientObjects => _ambientObjects ??= AmbientObject.Load(GameState);

    /// <summary>
    /// Gets the astral rifts.
    /// </summary>
    ImmutableArray<AstralRift>? _astralRifts;
    public ImmutableArray<AstralRift> AstralRifts => _astralRifts ??= GetAstralRifts();

    /// <summary>
    /// Gets the debris.
    /// </summary>
    ImmutableArray<Debris>? _debris;
    public ImmutableArray<Debris> Debris => _debris ??= Debris.Load(GameState);

    /// <summary>
    /// Gets the exhibits.
    /// </summary>
    ImmutableArray<Exhibit>? _exhibits;
    public ImmutableArray<Exhibit> Exhibits => _exhibits ??= GetExhibits();

    /// <summary>
    /// Gets whether this is an ironman save.
    /// </summary>
    public bool IsIronman => GetMetaValue("ironman").Equals("True", StringComparison.OrdinalIgnoreCase) || 
                             GetMetaValue("ironman").Equals("yes", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the empire name from the save file.
    /// </summary>
    public string EmpireName => GetMetaValue("name");

    /// <summary>
    /// Gets the number of fleets in the save file.
    /// </summary>
    public int FleetCount => int.TryParse(GetMetaValue("meta_fleets"), out var count) ? count : 0;

    /// <summary>
    /// Gets the number of planets in the save file.
    /// </summary>
    public int PlanetCount => GetPlanets().Length;

    /// <summary>
    /// Gets all first contacts in the save file.
    /// </summary>
    ImmutableArray<FirstContacts>? _firstContacts;
    public ImmutableArray<FirstContacts> FirstContacts => _firstContacts ??= GetFirstContacts();

    /// <summary>
    /// Gets all ships in the save file.
    /// </summary>
    ImmutableArray<Ship>? _ships;
    public ImmutableArray<Ship> Ships => _ships ??= GetShips();

    /// <summary>
    /// Gets all achievements in the save file.
    /// </summary>
    public Achievements Achievements => GetAchievements();

    /// <summary>
    /// Gets all planets in the save file.
    /// </summary>
    ImmutableArray<Planet>? _planets;
    public ImmutableArray<Planet> Planets => _planets ??= GetPlanets();

    /// <summary>
    /// Gets the megastructures.
    /// </summary>
    public ImmutableArray<Megastructure> Megastructures => _megastructures ??= Megastructure.Load(GameState);

    ImmutableArray<Megastructure>? _megastructures;

    /// <summary>
    /// Gets the archaeological sites.
    /// </summary>
    public ImmutableArray<ArchaeologicalSite> ArchaeologicalSites => _archaeologicalSites ??= ArchaeologicalSite.Load(GameState);

    ImmutableArray<ArchaeologicalSite>? _archaeologicalSites;

    /// <summary>
    /// Gets the espionage operations.
    /// </summary>
    public ImmutableArray<EspionageOperation> EspionageOperations => _espionageOperations ??= EspionageOperation.Load(GameState);

    ImmutableArray<EspionageOperation>? _espionageOperations;

    /// <summary>
    /// Gets the trade routes.
    /// </summary>
    public ImmutableArray<TradeRoute> TradeRoutes => _tradeRoutes ??= TradeRoute.Load(GameState);

    ImmutableArray<TradeRoute>? _tradeRoutes;

    /// <summary>
    /// Gets the bypasses.
    /// </summary>
    public ImmutableArray<Bypass> Bypasses => _bypasses ??= Bypass.Load(GameState);

    ImmutableArray<Bypass>? _bypasses;

    /// <summary>
    /// Gets the storms.
    /// </summary>
    public ImmutableArray<Storm> Storms => _storms ??= Storm.Load(GameState);

    ImmutableArray<Storm>? _storms;

    /// <summary>
    /// Gets the situations.
    /// </summary>
    public ImmutableArray<Situation> Situations => _situations ??= Situation.Load(GameState);

    ImmutableArray<Situation>? _situations;

    /// <summary>
    /// Gets the markets.
    /// </summary>
    public ImmutableArray<Market> Markets => _markets ??= Market.Load(GameState);

    ImmutableArray<Market>? _markets;

    /// <summary>
    /// Gets the clusters.
    /// </summary>
    public ImmutableArray<Cluster> Clusters => _clusters ??= Cluster.Load(GameState);

    ImmutableArray<Cluster>? _clusters;

    /// <summary>
    /// Gets the buildings.
    /// </summary>
    public ImmutableArray<Building> Buildings => _buildings ??= Building.Load(GameState);

    ImmutableArray<Building>? _buildings;

    /// <summary>
    /// Gets the constructions.
    /// </summary>
    public ImmutableArray<Construction> Constructions => _constructions ??= Construction.Load(GameState);

    ImmutableArray<Construction>? _constructions;

    /// <summary>
    /// Gets the deposits.
    /// </summary>
    public ImmutableArray<Deposit> Deposits => _deposits ??= Deposit.Load(GameState);

    ImmutableArray<Deposit>? _deposits;

    /// <summary>
    /// Gets the underlying game save documents.
    /// </summary>
    public GameSaveDocuments GetDocuments() => _documents;


    public static StellarisSave FromSave(string saveFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveFile, nameof(saveFile));

        if (string.IsNullOrEmpty(saveFile))
            throw new ArgumentException("Save file path cannot be null or empty", nameof(saveFile));

        if (!File.Exists(saveFile))        
            throw new FileNotFoundException("Stellaris save file not found", saveFile);
      

        try 
        {
            using( var stream = File.OpenRead(saveFile))
            {
                using( var zip = new GameSaveZip(stream))
                {
                    var documents = zip.GetDocuments();
                    var meta = documents.Meta.Root as SaveObject;
                    var gameState = documents.GameState.Root as SaveObject;

                    if (meta == null || gameState == null)
                        throw new InvalidDataException("Invalid save file format");

                    return new StellarisSave(documents);
                }
            }            
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Invalid save file format", ex);
        }

        throw new InvalidDataException("Invalid save file format");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StellarisSave"/> class.
    /// </summary>
    /// <param name="documents">The game save documents.</param>
    public StellarisSave(GameSaveDocuments documents)
    {
        _documents = documents;
        Meta = (SaveObject)documents.Meta.Root;
        GameState = (SaveObject)documents.GameState.Root;

        Version = GetMetaValue("version");
        Date = GetMetaValue("date");
        Player = GetMetaValue("player_name");
        GalaxyName = GetMetaValue("name");

        // Initialize fields to empty arrays
        _fleets = ImmutableArray<Fleet>.Empty;
        _formations = ImmutableArray<Formation>.Empty;
        _sectors = ImmutableArray<Sector>.Empty;
        _shipDesigns = ImmutableArray<ShipDesign>.Empty;
        _species = ImmutableArray<Species>.Empty;
        _armies = ImmutableArray<Army>.Empty;
        _ambientObjects = ImmutableArray<AmbientObject>.Empty;
        _astralRifts = ImmutableArray<AstralRift>.Empty;
        _debris = ImmutableArray<Debris>.Empty;
        _exhibits = ImmutableArray<Exhibit>.Empty;
        _firstContacts = ImmutableArray<FirstContacts>.Empty;
        _ships = ImmutableArray<Ship>.Empty;
        _planets = ImmutableArray<Planet>.Empty;
        _markets = ImmutableArray<Market>.Empty;
        _clusters = ImmutableArray<Cluster>.Empty;
        _buildings = ImmutableArray<Building>.Empty;
        _constructions = ImmutableArray<Construction>.Empty;
        _deposits = ImmutableArray<Deposit>.Empty;
    }

    string GetMetaValue(string key)
    {
        var property = Meta.Properties.FirstOrDefault(p => p.Key == key);
        
        if (property.Key != null)
        {
            System.Console.WriteLine($"Found key '{key}' with value type {property.Value?.GetType().Name}");
            
            // Handle different scalar types
            if (property.Value is Scalar<string> stringScalar)
            {
                System.Console.WriteLine($"Successfully cast '{key}' to Scalar<string> with value '{stringScalar.Value}'");
                return stringScalar.Value;
            }
            else if (property.Value is Scalar<int> intScalar)
            {
                System.Console.WriteLine($"Successfully cast '{key}' to Scalar<int> with value '{intScalar.Value}'");
                return intScalar.Value.ToString();
            }
            else if (property.Value is Scalar<bool> boolScalar)
            {
                System.Console.WriteLine($"Successfully cast '{key}' to Scalar<bool> with value '{boolScalar.Value}'");
                return boolScalar.Value.ToString();
            }
            else if (property.Value is Scalar<double> doubleScalar)
            {
                System.Console.WriteLine($"Successfully cast '{key}' to Scalar<double> with value '{doubleScalar.Value}'");
                return doubleScalar.Value.ToString();
            }
            else
            {
                System.Console.WriteLine($"Value for '{key}' is of unexpected type: {property.Value?.GetType().Name}");
                return property.Value?.ToString() ?? string.Empty;
            }
        }
        else
        {
            System.Console.WriteLine($"Key '{key}' not found");
        }
        return string.Empty;
    }

    ImmutableArray<FirstContacts> GetFirstContacts()
    {
        return Models.FirstContacts.Load(GameState).ToImmutableArray();
    }

    ImmutableArray<Ship> GetShips()
    {
        return Models.Ship.Load(GameState).ToImmutableArray();
    }

    LocalizedText GetLocalizedText(SaveObject obj, string context)
    {
        return Models.LocalizedText.Load(obj, context);
    }

    Achievements GetAchievements()
    {
        return Models.Achievements.Load(GameState);
    }

    ImmutableArray<Planet> GetPlanets()
    {
        return Models.Planet.Load(GameState).ToImmutableArray();
    }

    ImmutableArray<Species> GetSpecies()
    {
        return Models.Species.Load(GameState).ToImmutableArray();
    }

    ImmutableArray<Army> GetArmies()
    {
        return Models.Army.Load(GameState).ToImmutableArray();
    }

    ImmutableArray<AstralRift> GetAstralRifts()
    {
        return Models.AstralRift.Load(GameState).ToImmutableArray();
    }

    ImmutableArray<Exhibit> GetExhibits()
    {
        return Models.Exhibit.Load(GameState);
    }
} 