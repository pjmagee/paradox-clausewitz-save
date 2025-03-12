using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Cluster
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required Coordinate Coordinate { get; init; }
    public required float Radius { get; init; }
    public required ImmutableArray<int> Systems { get; init; }
    public required bool IsActive { get; init; }

    public static ImmutableArray<Cluster> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Cluster>();
        var clustersElement = root.Properties
            .FirstOrDefault(p => p.Key == "clusters");

        var clustersObj = clustersElement.Value as SaveObject;
        if (clustersObj != null)
        {
            foreach (var clusterElement in clustersObj.Properties)
            {
                if (long.TryParse(clusterElement.Key, out var clusterId))
                {
                    var obj = clusterElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var coordinate = Coordinate.Load(GetObject(obj, "coordinate"));
                    var radius = GetScalarFloat(obj, "radius");
                    var isActive = GetScalarBoolean(obj, "is_active");
                        
                    var systems = GetArray(obj, "systems")?.Items
                        .OfType<Scalar<int>>()
                        .Select(s => s.Value)
                        .ToImmutableArray() ?? ImmutableArray<int>.Empty;

                    if (type == null || coordinate == null)
                    {
                        continue;
                    }

                    builder.Add(new Cluster
                    {
                        Id = clusterId,
                        Type = type,
                        Coordinate = coordinate,
                        Radius = radius,
                        Systems = systems,
                        IsActive = isActive
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 