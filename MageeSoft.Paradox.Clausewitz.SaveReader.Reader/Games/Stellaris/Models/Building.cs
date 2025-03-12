using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Building
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Planet { get; init; }
    public required bool IsActive { get; init; }
    public required float Health { get; init; }
    public required float MaxHealth { get; init; }

    public static ImmutableArray<Building> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Building>();
        var buildingsElement = root.Properties
            .FirstOrDefault(p => p.Key == "buildings");

        var buildingsObj = buildingsElement.Value as SaveObject;
        if (buildingsObj != null)
        {
            foreach (var buildingElement in buildingsObj.Properties)
            {
                if (long.TryParse(buildingElement.Key, out var buildingId))
                {
                    var obj = buildingElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var planet = GetScalarInt(obj, "planet");
                    var isActive = GetScalarBoolean(obj, "is_active");
                    var health = GetScalarFloat(obj, "health");
                    var maxHealth = GetScalarFloat(obj, "max_health");

                    if (type == null)
                    {
                        continue;
                    }

                    builder.Add(new Building
                    {
                        Id = buildingId,
                        Type = type,
                        Planet = planet,
                        IsActive = isActive,
                        Health = health,
                        MaxHealth = maxHealth
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 