using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a pop in the game state.
/// </summary>
public record Pop
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the species id
    /// </summary>
    [SaveName("species")]
    public required int Species { get; init; }

    /// <summary>
    /// Gets or sets the faction.
    /// </summary>
    public required PopFaction Faction { get; init; }

    /// <summary>
    /// Gets or sets the happiness.
    /// </summary>
    public required float Happiness { get; init; }

    /// <summary>
    /// Gets or sets the power.
    /// </summary>
    public required float Power { get; init; }

    /// <summary>
    /// Default instance of Pop.
    /// </summary>
    public static Pop Default => new()
    {
        Id = 0,
        Species = 0,
        Faction = Models.PopFaction.Default,
        Happiness = 0f,
        Power = 0f
    };

    /// <summary>
    /// Loads a pop from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the pop data.</param>
    /// <returns>A new Pop instance.</returns>
    public static Pop? Load(SaveObject saveObject)
    {
        long id;
        //Species? species; // This is not in the gamestate-pop file
        PopFaction? faction;
        float happiness;
        float power;

        if (!saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetFloat("happiness", out happiness) ||
            !saveObject.TryGetFloat("power", out power))
        {
            return null;
        }

        // This is not in the gamestate-pop file
        // SaveObject? speciesObj;

        // if (!saveObject.TryGetSaveObject("species", out speciesObj) || speciesObj == null || (species = Species.LoadSingle(speciesObj)) == null)
        // {
        //     return null;
        // }

        saveObject.TryGetInt("species", out var species);

        SaveObject? factionObj;
        if (!saveObject.TryGetSaveObject("faction", out factionObj) || factionObj == null ||
            (faction = PopFaction.Load(factionObj)) == null)
        {
            return null;
        }

        return new Pop
        {
            Id = id,
            Species = species,
            Faction = faction,
            Happiness = happiness,
            Power = power
        };
    }
}






