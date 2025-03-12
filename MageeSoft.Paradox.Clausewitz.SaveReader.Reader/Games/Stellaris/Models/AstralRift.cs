using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an astral rift in the game state.
/// </summary>
public class AstralRift
{
    /// <summary>
    /// Gets or sets the astral rift ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the astral rift.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the astral rift is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the name of the astral rift.
    /// </summary>
    public LocalizedText Name { get; set; } = new();

    /// <summary>
    /// Gets or sets the coordinate of the astral rift.
    /// </summary>
    public Coordinate Coordinate { get; set; } = new();

    /// <summary>
    /// Gets or sets the owner ID.
    /// </summary>
    public int Owner { get; set; }

    /// <summary>
    /// Gets or sets the explorer fleet ID.
    /// </summary>
    public long ExplorerFleet { get; set; }

    /// <summary>
    /// Gets or sets the leader ID.
    /// </summary>
    public long Leader { get; set; }

    /// <summary>
    /// Gets or sets the explorer ID.
    /// </summary>
    public long Explorer { get; set; }

    /// <summary>
    /// Gets or sets the number of clues.
    /// </summary>
    public int Clues { get; set; }

    /// <summary>
    /// Gets or sets the last roll value.
    /// </summary>
    public int LastRoll { get; set; }

    /// <summary>
    /// Gets or sets the days left.
    /// </summary>
    public int DaysLeft { get; set; }

    /// <summary>
    /// Gets or sets the difficulty.
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Gets or sets the event information.
    /// </summary>
    public AstralRiftEvent Event { get; set; } = new();

    /// <summary>
    /// Gets or sets the event choice.
    /// </summary>
    public string EventChoice { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the on roll failed value.
    /// </summary>
    public string OnRollFailed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fail probability.
    /// </summary>
    public int FailProbability { get; set; }

    /// <summary>
    /// Gets or sets the cumulated fail probability.
    /// </summary>
    public int CumulatedFailProbability { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the flags.
    /// </summary>
    public Dictionary<string, long> Flags { get; set; } = new();

    /// <summary>
    /// Gets or sets the interactable by IDs.
    /// </summary>
    public List<int> InteractableBy { get; set; } = new();

    /// <summary>
    /// Gets or sets the astral rift orbitals.
    /// </summary>
    public List<object> AstralRiftOrbitals { get; set; } = new();

    /// <summary>
    /// Gets or sets the ship class orbital station ID.
    /// </summary>
    public long ShipClassOrbitalStation { get; set; }
    
    public static IReadOnlyList<AstralRift> Load(SaveObject root)
    {
        var astralRifts = new List<AstralRift>();
        
        var astralRiftsElement = root.Properties.FirstOrDefault(p => p.Key == "astral_rifts");
        if (astralRiftsElement.Value is not SaveObject astralRiftsObj)
        {
            return astralRifts;
        }

        var riftsElement = astralRiftsObj.Properties.FirstOrDefault(p => p.Key == "rifts");
        if (riftsElement.Value is not SaveObject riftsObj)
        {
            return astralRifts;
        }

        foreach (var riftElement in riftsObj.Properties)
        {
            if (long.TryParse(riftElement.Key, out var riftId))
            {
                var rift = new AstralRift { Id = riftId };
                
                if (riftElement.Value is SaveObject properties)
                {
                    foreach (var property in properties.Properties)
                    {
                        switch (property.Key)
                        {
                            case "name" when property.Value is SaveObject nameObj:
                                rift.Name = LocalizedText.Load(nameObj, "name");
                                break;
                            case "coordinate" when property.Value is SaveObject coordObj:
                                rift.Coordinate = Coordinate.Load(coordObj);
                                break;
                            case "owner" when property.Value?.TryGetScalar<int>(out var value) == true:
                                rift.Owner = value;
                                break;
                            case "explorer_fleet" when property.Value?.TryGetScalar<long>(out var value) == true:
                                rift.ExplorerFleet = value;
                                break;
                            case "leader" when property.Value?.TryGetScalar<long>(out var value) == true:
                                rift.Leader = value;
                                break;
                            case "explorer" when property.Value?.TryGetScalar<long>(out var value) == true:
                                rift.Explorer = value;
                                break;
                            case "clues" when property.Value?.TryGetScalar<int>(out var value) == true:
                                rift.Clues = value;
                                break;
                            case "last_roll" when property.Value?.TryGetScalar<int>(out var value) == true:
                                rift.LastRoll = value;
                                break;
                            case "days_left" when property.Value?.TryGetScalar<int>(out var value) == true:
                                rift.DaysLeft = value;
                                break;
                            case "difficulty" when property.Value?.TryGetScalar<int>(out var value) == true:
                                rift.Difficulty = value;
                                break;
                            case "event" when property.Value is SaveObject eventObj:
                                rift.Event = LoadEvent(eventObj);
                                break;
                            case "event_choice" when property.Value?.TryGetScalar<string>(out var value) == true:
                                rift.EventChoice = value;
                                break;
                            case "on_roll_failed" when property.Value?.TryGetScalar<string>(out var value) == true:
                                rift.OnRollFailed = value;
                                break;
                            case "fail_probability" when property.Value?.TryGetScalar<int>(out var value) == true:
                                rift.FailProbability = value;
                                break;
                            case "cumulated_fail_probability" when property.Value?.TryGetScalar<int>(out var value) == true:
                                rift.CumulatedFailProbability = value;
                                break;
                            case "status" when property.Value?.TryGetScalar<string>(out var value) == true:
                                rift.Status = value;
                                break;
                            case "flags" when property.Value is SaveObject flagsObj:
                                rift.Flags = LoadFlags(flagsObj);
                                break;
                            case "interactable_by" when property.Value is SaveArray interactableArray:
                                rift.InteractableBy = LoadInteractableBy(interactableArray);
                                break;
                            case "astral_rift_orbitals" when property.Value is SaveObject orbitalsObj:
                                rift.AstralRiftOrbitals = new List<object>(); // TODO: Implement orbital loading
                                break;
                            case "shipclass_orbital_station" when property.Value?.TryGetScalar<long>(out var value) == true:
                                rift.ShipClassOrbitalStation = value;
                                break;
                        }
                    }
                }

                astralRifts.Add(rift);
            }
        }

        return astralRifts;
    }

    static AstralRiftEvent LoadEvent(SaveObject eventObj)
    {
        var evt = new AstralRiftEvent();
        
        foreach (var property in eventObj.Properties)
        {
            switch (property.Key)
            {
                case "scope" when property.Value is SaveObject scopeObj:
                    evt.Scope = LoadEventScope(scopeObj);
                    break;
                case "effect" when property.Value?.TryGetScalar<string>(out var value) == true:
                    evt.Effect = value;
                    break;
                case "picture" when property.Value?.TryGetScalar<string>(out var value) == true:
                    evt.Picture = value;
                    break;
                case "index" when property.Value?.TryGetScalar<int>(out var value) == true:
                    evt.Index = value;
                    break;
            }
        }

        return evt;
    }

    static AstralRiftEventScope LoadEventScope(SaveObject scopeObj)
    {
        var scope = new AstralRiftEventScope();
        
        foreach (var property in scopeObj.Properties)
        {
            switch (property.Key)
            {
                case "type" when property.Value?.TryGetScalar<string>(out var value) == true:
                    scope.Type = value;
                    break;
                case "id" when property.Value?.TryGetScalar<long>(out var value) == true:
                    scope.Id = value;
                    break;
                case "opener_id" when property.Value?.TryGetScalar<long>(out var value) == true:
                    scope.OpenerId = value;
                    break;
                case "random" when property.Value is SaveArray randomArray:
                    scope.Random = LoadRandomValues(randomArray);
                    break;
                case "random_allowed" when property.Value?.TryGetScalar<bool>(out var value) == true:
                    scope.RandomAllowed = value;
                    break;
            }
        }

        return scope;
    }

    static List<long> LoadRandomValues(SaveArray randomArray)
    {
        var values = new List<long>();
        foreach (var element in randomArray.Items)
        {
            if (element.TryGetScalar<long>(out var value))
            {
                values.Add(value);
            }
        }
        return values;
    }

    static Dictionary<string, long> LoadFlags(SaveObject flagsObj)
    {
        var flags = new Dictionary<string, long>();
        foreach (var property in flagsObj.Properties)
        {
            if (property.Value?.TryGetScalar<long>(out var value) == true)
            {
                flags[property.Key] = value;
            }
        }
        return flags;
    }

    static List<int> LoadInteractableBy(SaveArray interactableArray)
    {
        var values = new List<int>();
        foreach (var element in interactableArray.Items)
        {
            if (element.TryGetScalar<int>(out var value))
            {
                values.Add(value);
            }
        }
        return values;
    }
}

/// <summary>
/// Represents an astral rift event.
/// </summary>
public class AstralRiftEvent
{
    /// <summary>
    /// Gets or sets the scope information.
    /// </summary>
    public AstralRiftEventScope Scope { get; set; } = new();

    /// <summary>
    /// Gets or sets the effect.
    /// </summary>
    public string Effect { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the picture.
    /// </summary>
    public string Picture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public int Index { get; set; }
}

/// <summary>
/// Represents the scope of an astral rift event.
/// </summary>
public class AstralRiftEventScope
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the opener ID.
    /// </summary>
    public long OpenerId { get; set; }

    /// <summary>
    /// Gets or sets the random values.
    /// </summary>
    public List<long> Random { get; set; } = new();

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public bool RandomAllowed { get; set; }
}

/// <summary>
/// Provides methods for loading astral rifts from game save documents.
/// </summary>
public static class AstralRiftLoader
{
    
} 