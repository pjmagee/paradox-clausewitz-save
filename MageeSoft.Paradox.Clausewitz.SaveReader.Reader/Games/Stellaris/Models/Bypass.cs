using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Bypass
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required Coordinate Coordinate { get; init; }
    public required ImmutableArray<int> ConnectedSystems { get; init; }
    public required ImmutableArray<int> DiscoveredBy { get; init; }

    public static ImmutableArray<Bypass> Load(GameSaveDocuments documents)
    {
        var builder = ImmutableArray.CreateBuilder<Bypass>();
        var bypassesElement = (documents.GameState.Root as SaveObject)?.Properties
            .FirstOrDefault(p => p.Key == "bypasses");

        if (bypassesElement.HasValue)
        {
            var bypassesObj = bypassesElement.Value.Value as SaveObject;
            if (bypassesObj != null)
            {
                foreach (var bypassElement in bypassesObj.Properties)
                {
                    if (long.TryParse(bypassElement.Key, out var bypassId))
                    {
                        var obj = bypassElement.Value as SaveObject;
                        if (obj == null)
                        {
                            continue;
                        }

                        var type = GetScalarString(obj, "type");
                        var coordinate = Coordinate.Load(GetObject(obj, "coordinate"));
                        
                        var connectedSystems = GetArray(obj, "connected_systems")?.Items
                            .OfType<Scalar<int>>()
                            .Select(s => s.Value)
                            .ToImmutableArray() ?? ImmutableArray<int>.Empty;

                        var discoveredBy = GetArray(obj, "discovered_by")?.Items
                            .OfType<Scalar<int>>()
                            .Select(s => s.Value)
                            .ToImmutableArray() ?? ImmutableArray<int>.Empty;

                        if (type == null || coordinate == null)
                        {
                            continue;
                        }

                        builder.Add(new Bypass
                        {
                            Id = bypassId,
                            Type = type,
                            Coordinate = coordinate,
                            ConnectedSystems = connectedSystems,
                            DiscoveredBy = discoveredBy
                        });
                    }
                }
            }
        }

        return builder.ToImmutable();
    }
} 