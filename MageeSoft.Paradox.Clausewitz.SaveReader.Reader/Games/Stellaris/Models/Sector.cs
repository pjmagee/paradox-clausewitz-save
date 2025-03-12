using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a sector in the game state.
/// </summary>
public class Sector
{
    /// <summary>
    /// Gets or sets the sector ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the sector name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the sector owner.
    /// </summary>
    public long Owner { get; init; }

    /// <summary>
    /// Gets or sets the sector type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the sector capital.
    /// </summary>
    public long Capital { get; init; }

    /// <summary>
    /// Gets or sets the sector systems.
    /// </summary>
    public ImmutableArray<long> Systems { get; init; } = ImmutableArray<long>.Empty;

    /// <summary>
    /// Gets or sets the sector resources.
    /// </summary>
    public ImmutableDictionary<string, float> Resources { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Gets or sets the sector stockpile.
    /// </summary>
    public ImmutableDictionary<string, float> Stockpile { get; init; } = ImmutableDictionary<string, float>.Empty;

    /// <summary>
    /// Loads all sectors from the game state document.
    /// </summary>
    /// <param name="gameState">The game state document to load from.</param>
    /// <returns>An immutable array of sectors.</returns>
    public static ImmutableArray<Sector> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Sector>();

        var sectorsObj = GetObject(root, "sectors");
            if (sectorsObj != null)
            {
                foreach (var property in sectorsObj.Properties)
                {
                    if (long.TryParse(property.Key, out var sectorId) && property.Value is SaveObject sectorObj)
                    {
                        var systemsBuilder = ImmutableArray.CreateBuilder<long>();
                        var resourcesBuilder = ImmutableDictionary.CreateBuilder<string, float>();
                        var stockpileBuilder = ImmutableDictionary.CreateBuilder<string, float>();

                        if (GetArray(sectorObj, "systems") is SaveArray systemsArray)
                        {
                            foreach (var system in systemsArray.Items)
                            {
                                if (system is Scalar<long> systemScalar)
                                {
                                    systemsBuilder.Add(systemScalar.Value);
                                }
                            }
                        }

                        if (GetObject(sectorObj, "resources") is SaveObject resourcesObj)
                        {
                            foreach (var resource in resourcesObj.Properties)
                            {
                                if (resource.Value is Scalar<float> resourceScalar)
                                {
                                    resourcesBuilder.Add(resource.Key, resourceScalar.Value);
                                }
                            }
                        }

                        if (GetObject(sectorObj, "stockpile") is SaveObject stockpileObj)
                        {
                            foreach (var stockpile in stockpileObj.Properties)
                            {
                                if (stockpile.Value is Scalar<float> stockpileScalar)
                                {
                                    stockpileBuilder.Add(stockpile.Key, stockpileScalar.Value);
                                }
                            }
                        }

                        var sector = new Sector
                        {
                            Id = sectorId,
                            Name = GetScalarString(sectorObj, "name"),
                            Owner = GetScalarLong(sectorObj, "owner"),
                            Type = GetScalarString(sectorObj, "type"),
                            Capital = GetScalarLong(sectorObj, "capital"),
                            Systems = systemsBuilder.ToImmutable(),
                            Resources = resourcesBuilder.ToImmutable(),
                            Stockpile = stockpileBuilder.ToImmutable()
                        };

                        builder.Add(sector);
                    }
                }
            }

        return builder.ToImmutable();
    }
} 