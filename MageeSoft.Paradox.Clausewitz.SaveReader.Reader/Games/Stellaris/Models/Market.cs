using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Market
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Owner { get; init; }
    public required ImmutableDictionary<string, float> Resources { get; init; }
    public required ImmutableDictionary<string, float> Prices { get; init; }
    public required ImmutableDictionary<string, float> Demand { get; init; }

    public static ImmutableArray<Market> Load(GameStateDocument document)
    {
        var builder = ImmutableArray.CreateBuilder<Market>();
        
        var marketsElement = (document.Root as SaveObject)?.Properties.FirstOrDefault(p => p.Key == "galactic_market");

        if (marketsElement.HasValue)
        {
            var marketsObj = marketsElement.Value.Value as SaveObject;
            if (marketsObj != null)
            {
                foreach (var marketElement in marketsObj.Properties)
                {
                    if (long.TryParse(marketElement.Key, out var marketId))
                    {
                        var obj = marketElement.Value as SaveObject;
                        if (obj == null)
                        {
                            continue;
                        }

                        var type = GetScalarString(obj, "type");
                        var owner = GetScalarInt(obj, "owner");

                        var resources = GetObject(obj, "resources")?.Properties.ToImmutableDictionary(
                            kvp => kvp.Key,
                            kvp => (kvp.Value as Scalar<float>)?.Value ?? 0
                        ) ?? ImmutableDictionary<string, float>.Empty;

                        var prices = GetObject(obj, "prices")?.Properties.ToImmutableDictionary(
                            kvp => kvp.Key,
                            kvp => (kvp.Value as Scalar<float>)?.Value ?? 0
                        ) ?? ImmutableDictionary<string, float>.Empty;

                        var demand = GetObject(obj, "demand")?.Properties.ToImmutableDictionary(
                            kvp => kvp.Key,
                            kvp => (kvp.Value as Scalar<float>)?.Value ?? 0
                        ) ?? ImmutableDictionary<string, float>.Empty;

                        if (type == null)
                        {
                            continue;
                        }

                        builder.Add(new Market
                        {
                            Id = marketId,
                            Type = type,
                            Owner = owner,
                            Resources = resources,
                            Prices = prices,
                            Demand = demand
                        });
                    }
                }
            }
        }

        return builder.ToImmutable();
    }
} 