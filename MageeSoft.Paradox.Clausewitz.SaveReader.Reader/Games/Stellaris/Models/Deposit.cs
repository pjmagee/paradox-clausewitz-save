using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Deposit
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Planet { get; init; }
    public required bool IsActive { get; init; }
    public required ImmutableDictionary<string, float> Resources { get; init; }

    public static ImmutableArray<Deposit> Load(GameSaveDocuments documents)
    {
        var builder = ImmutableArray.CreateBuilder<Deposit>();
        var depositsElement = (documents.GameState.Root as SaveObject)?.Properties
            .FirstOrDefault(p => p.Key == "deposits");

        if (depositsElement.HasValue)
        {
            var depositsObj = depositsElement.Value.Value as SaveObject;
            if (depositsObj != null)
            {
                foreach (var depositElement in depositsObj.Properties)
                {
                    if (long.TryParse(depositElement.Key, out var depositId))
                    {
                        var obj = depositElement.Value as SaveObject;
                        if (obj == null)
                        {
                            continue;
                        }

                        var type = GetScalarString(obj, "type");
                        var planet = GetScalarInt(obj, "planet");
                        var isActive = GetScalarBoolean(obj, "is_active");
                        
                        var resources = GetObject(obj, "resources")?.Properties.ToImmutableDictionary(
                            kvp => kvp.Key,
                            kvp => (kvp.Value as Scalar<float>)?.Value ?? 0
                        ) ?? ImmutableDictionary<string, float>.Empty;

                        if (type == null)
                        {
                            continue;
                        }

                        builder.Add(new Deposit
                        {
                            Id = depositId,
                            Type = type,
                            Planet = planet,
                            IsActive = isActive,
                            Resources = resources
                        });
                    }
                }
            }
        }

        return builder.ToImmutable();
    }
} 