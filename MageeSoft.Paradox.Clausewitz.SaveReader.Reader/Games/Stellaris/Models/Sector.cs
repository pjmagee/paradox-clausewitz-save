using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a sector in the game state.
/// </summary>
public record Sector
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required Owner Owner { get; init; }

    /// <summary>
    /// Gets or sets the planets.
    /// </summary>
    public required ImmutableArray<Planet> Planets { get; init; }

    /// <summary>
    /// Default instance of Sector.
    /// </summary>
    public static Sector Default => new()
    {
        Id = 0,
        Name = string.Empty,
        Type = string.Empty,
        Owner = Owner.Default,
        Planets = ImmutableArray<Planet>.Empty
    };

    /// <summary>
    /// Loads all sectors from the game state document.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of sectors.</returns>
    public static ImmutableArray<Sector> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Sector>();

        SaveObject? sectorsObj;
        if (!root.TryGetSaveObject("sectors", out sectorsObj) || sectorsObj == null)
        {
            return builder.ToImmutable();
        }

        foreach (var sectorElement in sectorsObj.Properties)
        {
            if (long.TryParse(sectorElement.Key, out var sectorId) && sectorElement.Value is SaveObject sectorObj)
            {
                string name;
                string type;
                Owner? owner;

                if (!sectorObj.TryGetString("name", out name) ||
                    !sectorObj.TryGetString("type", out type))
                {
                    continue;
                }

                SaveObject? ownerObj;
                if (!sectorObj.TryGetSaveObject("owner", out ownerObj) || ownerObj == null ||
                    (owner = Owner.Load(ownerObj)) == null)
                {
                    continue;
                }

                SaveObject? planetsObj;
                if (!sectorObj.TryGetSaveObject("planets", out planetsObj))
                {
                    continue;
                }

                var planets = planetsObj?.Properties.Select(p => p.Value)
                    .OfType<SaveObject>()
                    .Select(Planet.Load)
                    .Where(planet => planet != null)
                    .ToImmutableArray() ?? ImmutableArray<Planet>.Empty;

                builder.Add(new Sector
                {
                    Id = sectorId,
                    Name = name,
                    Type = type,
                    Owner = owner,
                    Planets = planets!
                });
            }
        }

        return builder.ToImmutable();
    }
} 






