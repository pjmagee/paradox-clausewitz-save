using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents debris in the game state.
/// </summary>
public class Debris
{
    /// <summary>
    /// Gets or sets the debris ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the country ID that owns the debris.
    /// </summary>
    public int Country { get; init; }

    /// <summary>
    /// Gets or sets the country ID that the debris came from.
    /// </summary>
    public int FromCountry { get; init; }

    /// <summary>
    /// Gets or sets the coordinate of the debris.
    /// </summary>
    public Coordinate Coordinate { get; init; } = new();

    /// <summary>
    /// Gets or sets the resources in the debris.
    /// </summary>
    public ImmutableDictionary<string, int> Resources { get; init; } = ImmutableDictionary<string, int>.Empty;

    /// <summary>
    /// Gets or sets the ship sizes in the debris.
    /// </summary>
    public ImmutableArray<string> ShipSizes { get; init; } = ImmutableArray<string>.Empty;

    /// <summary>
    /// Gets or sets the components in the debris.
    /// </summary>
    public ImmutableArray<string> Components { get; init; } = ImmutableArray<string>.Empty;

    /// <summary>
    /// Gets or sets the date of the debris.
    /// </summary>
    public string Date { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the debris must be scavenged.
    /// </summary>
    public bool MustScavenge { get; init; }

    /// <summary>
    /// Gets or sets whether the debris must be reanimated.
    /// </summary>
    public bool MustReanimate { get; init; }

    /// <summary>
    /// Gets or sets whether the debris must be researched.
    /// </summary>
    public bool MustResearch { get; init; }

    /// <summary>
    /// Loads all debris from the game state.
    /// </summary>
    /// <param name="gameState">The game state root object to load from.</param>
    /// <returns>An immutable array of debris.</returns>
    public static ImmutableArray<Debris> Load(SaveObject gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState, nameof(gameState));
        
        var builder = ImmutableArray.CreateBuilder<Debris>();

        var debrisElement = gameState.Properties.FirstOrDefault(p => p.Key == "debris");
        if (debrisElement.Value is not SaveObject debrisObj)
        {
            System.Console.WriteLine("No debris object found in root properties");
            return ImmutableArray<Debris>.Empty;
        }

        System.Console.WriteLine($"Found debris object with {debrisObj.Properties.Length} properties");

        foreach (var debrisItem in debrisObj.Properties)
        {
            if (long.TryParse(debrisItem.Key, out var debrisId))
            {
                // Skip 'none' values
                if (debrisItem.Value is Scalar<string> scalar && scalar.Value == "none")
                {
                    System.Console.WriteLine($"Skipping debris {debrisId} (none)");
                    continue;
                }

                if (debrisItem.Value is SaveObject properties)
                {
                    var resourcesBuilder = ImmutableDictionary.CreateBuilder<string, int>();
                    var shipSizesBuilder = ImmutableArray.CreateBuilder<string>();
                    var componentsBuilder = ImmutableArray.CreateBuilder<string>();

                    var debris = new Debris
                    {
                        Id = debrisId,
                        Country = GetScalarInt(properties, "country"),
                        FromCountry = GetScalarInt(properties, "from_country"),
                        Coordinate = Coordinate.Load(GetObject(properties, "coordinate")),
                        Date = GetScalarString(properties, "date") ?? string.Empty,
                        MustScavenge = GetScalarBoolean(properties, "must_scavenge"),
                        MustReanimate = GetScalarBoolean(properties, "must_reanimate"),
                        MustResearch = GetScalarBoolean(properties, "must_research")
                    };

                    // Load resources
                    var resourcesObj = GetObject(properties, "resources");
                    if (resourcesObj != null)
                    {
                        foreach (var resource in resourcesObj.Properties)
                        {
                            if (resource.Value is SaveObject resourceObj)
                            {
                                foreach (var resourceProperty in resourceObj.Properties)
                                {
                                    if (resourceProperty.Value?.TryGetScalar<string>(out var resourceText) == true)
                                    {
                                        var parts = resourceText.Split(' ');
                                        if (parts.Length >= 2 && int.TryParse(parts[1], out var amount))
                                        {
                                            resourcesBuilder[parts[0]] = amount;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Load ship sizes and components
                    foreach (var property in properties.Properties)
                    {
                        if (property.Key == "ship_size" && property.Value?.TryGetScalar<string>(out var shipSize) == true)
                        {
                            shipSizesBuilder.Add(shipSize);
                        }
                        else if (property.Key == "component" && property.Value?.TryGetScalar<string>(out var component) == true)
                        {
                            componentsBuilder.Add(component);
                        }
                    }

                    // debris = debris with
                    // {
                    //     Resources = resourcesBuilder.ToImmutable(),
                    //     ShipSizes = shipSizesBuilder.ToImmutable(),
                    //     Components = componentsBuilder.ToImmutable()
                    // };

                    builder.Add(debris);
                    System.Console.WriteLine($"Added debris with ID {debrisId}, Country: {debris.Country}, FromCountry: {debris.FromCountry}");
                }
                else
                {
                    System.Console.WriteLine($"Skipping debris {debrisId} (not a SaveObject)");
                }
            }
        }

        System.Console.WriteLine($"Total debris loaded: {builder.Count}");
        return builder.ToImmutable();
    }
} 