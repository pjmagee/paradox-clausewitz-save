using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Storm
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required float Radius { get; init; }
    public required Coordinate Coordinate { get; init; }
    public required ImmutableArray<int> Systems { get; init; }

    public static ImmutableArray<Storm> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Storm>();
        var stormsElement = root.Properties
            .FirstOrDefault(p => p.Key == "storms");

        var stormsObj = stormsElement.Value as SaveObject;
        if (stormsObj != null)
        {
            foreach (var stormElement in stormsObj.Properties)
            {
                if (long.TryParse(stormElement.Key, out var stormId))
                {
                    var obj = stormElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var radius = GetScalarFloat(obj, "radius");
                    var coordinate = Coordinate.Load(GetObject(obj, "coordinate"));
                        
                    var systems = GetArray(obj, "systems")?.Items
                        .OfType<Scalar<int>>()
                        .Select(s => s.Value)
                        .ToImmutableArray() ?? ImmutableArray<int>.Empty;

                    if (type == null || coordinate == null)
                    {
                        continue;
                    }

                    builder.Add(new Storm
                    {
                        Id = stormId,
                        Type = type,
                        Radius = radius,
                        Coordinate = coordinate,
                        Systems = systems
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 