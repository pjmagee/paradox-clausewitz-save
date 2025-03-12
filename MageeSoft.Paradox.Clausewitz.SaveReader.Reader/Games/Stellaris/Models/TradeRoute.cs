using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record TradeRoute
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Source { get; init; }
    public required int Destination { get; init; }
    public required ImmutableDictionary<string, int> Resources { get; init; }

    public static ImmutableArray<TradeRoute> Load(GameSaveDocuments documents)
    {
        var builder = ImmutableArray.CreateBuilder<TradeRoute>();
        var routesElement = (documents.GameState.Root as SaveObject)?.Properties
            .FirstOrDefault(p => p.Key == "trade_routes");

        if (routesElement.HasValue)
        {
            var routesObj = routesElement.Value.Value as SaveObject;
            if (routesObj != null)
            {
                foreach (var routeElement in routesObj.Properties)
                {
                    if (long.TryParse(routeElement.Key, out var routeId))
                    {
                        var obj = routeElement.Value as SaveObject;
                        if (obj == null)
                        {
                            continue;
                        }

                        var type = GetScalarString(obj, "type");
                        var source = GetScalarInt(obj, "source");
                        var destination = GetScalarInt(obj, "destination");
                        
                        var resources = GetObject(obj, "resources")?.Properties.ToImmutableDictionary(
                            kvp => kvp.Key,
                            kvp => (kvp.Value as Scalar<int>)?.Value ?? 0
                        ) ?? ImmutableDictionary<string, int>.Empty;

                        if (type == null)
                        {
                            continue;
                        }

                        builder.Add(new TradeRoute
                        {
                            Id = routeId,
                            Type = type,
                            Source = source,
                            Destination = destination,
                            Resources = resources
                        });
                    }
                }
            }
        }

        return builder.ToImmutable();
    }
} 