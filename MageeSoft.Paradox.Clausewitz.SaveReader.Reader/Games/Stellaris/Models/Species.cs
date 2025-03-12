using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a species in the game state.
/// </summary>
public class Species
{
    /// <summary>
    /// Gets or sets the species ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the name of the species.
    /// </summary>
    public LocalizedText Name { get; init; } = new();

    /// <summary>
    /// Gets or sets the plural name of the species.
    /// </summary>
    public LocalizedText NamePlural { get; init; } = new();

    /// <summary>
    /// Gets or sets the adjective of the species.
    /// </summary>
    public LocalizedText Adjective { get; init; } = new();

    /// <summary>
    /// Gets or sets the class of the species.
    /// </summary>
    public string Class { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the portrait of the species.
    /// </summary>
    public string Portrait { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the name list of the species.
    /// </summary>
    public string NameList { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the traits of the species.
    /// </summary>
    public ImmutableArray<string> Traits { get; init; } = ImmutableArray<string>.Empty;

    /// <summary>
    /// Gets or sets the home planet of the species.
    /// </summary>
    public long HomePlanet { get; init; }

    /// <summary>
    /// Gets or sets the gender of the species.
    /// </summary>
    public string Gender { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the extra trait points of the species.
    /// </summary>
    public int ExtraTraitPoints { get; init; }

    /// <summary>
    /// Gets or sets the flags of the species.
    /// </summary>
    public ImmutableDictionary<string, long> Flags { get; init; } = ImmutableDictionary<string, long>.Empty;

    /// <summary>
    /// Gets or sets the name data of the species.
    /// </summary>
    public string NameData { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the base reference of the species.
    /// </summary>
    public long? BaseRef { get; init; }

    /// <summary>
    /// Loads all species from the game state document.
    /// </summary>
    /// <param name="gameState">The game state document to load from.</param>
    /// <returns>An immutable array of species.</returns>
    public static ImmutableArray<Species> Load(GameStateDocument gameState)
    {
        var builder = ImmutableArray.CreateBuilder<Species>();

        if (gameState.Root is SaveObject root)
        {
            var speciesDbElement = root.Properties.FirstOrDefault(p => p.Key == "species_db").Value as SaveObject;
            if (speciesDbElement != null)
            {
                foreach (var speciesElement in speciesDbElement.Properties)
                {
                    if (long.TryParse(speciesElement.Key, out var speciesId) && speciesElement.Value is SaveObject speciesObj)
                    {
                        var traitsBuilder = ImmutableArray.CreateBuilder<string>();
                        var flagsBuilder = ImmutableDictionary.CreateBuilder<string, long>();

                        foreach (var property in speciesObj.Properties)
                        {
                            switch (property.Key)
                            {
                                case "trait" when property.Value is Scalar<string> traitScalar:
                                    traitsBuilder.Add(traitScalar.Value);
                                    break;
                                case "flags" when property.Value is SaveObject flagsObj:
                                    foreach (var flag in GetFlags(flagsObj))
                                    {
                                        flagsBuilder.Add(flag.Key, flag.Value);
                                    }
                                    break;
                            }
                        }

                        var species = new Species
                        {
                            Id = speciesId,
                            Name = GetObject(speciesObj, "name") is SaveObject nameObj ? LocalizedText.Load(nameObj) : new(),
                            NamePlural = GetObject(speciesObj, "name_plural") is SaveObject namePluralObj ? LocalizedText.Load(namePluralObj) : new(),
                            Adjective = GetObject(speciesObj, "adjective") is SaveObject adjectiveObj ? LocalizedText.Load(adjectiveObj) : new(),
                            Class = GetScalarString(speciesObj, "class"),
                            Portrait = GetScalarString(speciesObj, "portrait"),
                            NameList = GetScalarString(speciesObj, "name_list"),
                            Traits = traitsBuilder.ToImmutable(),
                            HomePlanet = GetScalarLong(speciesObj, "home_planet"),
                            Gender = GetScalarString(speciesObj, "gender"),
                            ExtraTraitPoints = GetScalarInt(speciesObj, "extra_trait_points"),
                            Flags = flagsBuilder.ToImmutable(),
                            NameData = GetScalarString(speciesObj, "name_data"),
                            //BaseRef = GetScalarLongOrNull(speciesObj, "base_ref")
                        };

                        builder.Add(species);
                    }
                }
            }
        }

        return builder.ToImmutable();
    }

    private static Dictionary<string, long> GetFlags(SaveObject obj)
    {
        var flags = new Dictionary<string, long>();
        var flagsElement = obj.Properties.FirstOrDefault(p => p.Key == "flags");

        if (flagsElement.Key != null && flagsElement.Value is SaveObject flagsObj)
        {
            foreach (var flagElement in flagsObj.Properties)
            {
                if (flagElement.Value is Scalar<long> scalar)
                {
                    flags[flagElement.Key] = scalar.Value;
                }
                else if (flagElement.Value is Scalar<string> strScalar && long.TryParse(strScalar.Value, out var value))
                {
                    flags[flagElement.Key] = value;
                }
                else if (long.TryParse(flagElement.Value.ToString().Replace("Scalar: ", ""), out var parsed))
                {
                    flags[flagElement.Key] = parsed;
                }
            }
        }

        return flags;
    }
} 