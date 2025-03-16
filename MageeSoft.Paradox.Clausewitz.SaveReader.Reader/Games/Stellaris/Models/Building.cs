using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Building
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int RuinTime { get; init; }

    public static ImmutableArray<Building> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Building>();

        if (!root.TryGetSaveObject("buildings", out var buildingsObj))
        {
            return builder.ToImmutable();
        }

        foreach (var buildingElement in buildingsObj.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
        {
            if (long.TryParse(buildingElement.Key, out var buildingId) && buildingElement.Value is SaveObject obj)
            {
                var building = LoadSingle(obj);
                if (building != null)
                {
                    builder.Add(building with { Id = buildingId });
                }
            }
        }

        return builder.ToImmutable();
    }

    public static Building? LoadSingle(SaveObject obj)
    {
        string typeValue;
        int ruinTimeValue;

        if (!obj.TryGetString("type", out typeValue) || !obj.TryGetInt("ruin_time", out ruinTimeValue))
        {
            return null;
        }

        return new Building
        {
            Id = 0, // Will be set by the caller
            Type = typeValue,
            RuinTime = ruinTimeValue
        };
    }
} 






