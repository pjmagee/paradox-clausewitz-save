using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Construction
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Planet { get; init; }
    public required float Progress { get; init; }
    public required bool IsActive { get; init; }
    public required ImmutableDictionary<string, float> Resources { get; init; }

    public static ImmutableArray<Construction> Load(GameSaveDocuments documents)
    {
        var builder = ImmutableArray.CreateBuilder<Construction>();
        var constructionsElement = (documents.GameState.Root as SaveObject)?.Properties
            .FirstOrDefault(p => p.Key == "constructions");

        if (constructionsElement.HasValue)
        {
            var constructionsObj = constructionsElement.Value.Value as SaveObject;
            if (constructionsObj != null)
            {
                foreach (var constructionElement in constructionsObj.Properties)
                {
                    if (long.TryParse(constructionElement.Key, out var constructionId))
                    {
                        var obj = constructionElement.Value as SaveObject;
                        if (obj == null)
                        {
                            continue;
                        }

                        var type = GetScalarString(obj, "type");
                        var planet = GetScalarInt(obj, "planet");
                        var progress = GetScalarFloat(obj, "progress");
                        var isActive = GetScalarBoolean(obj, "is_active");
                        
                        var resources = GetObject(obj, "resources")?.Properties.ToImmutableDictionary(
                            kvp => kvp.Key,
                            kvp => (kvp.Value as Scalar<float>)?.Value ?? 0
                        ) ?? ImmutableDictionary<string, float>.Empty;

                        if (type == null)
                        {
                            continue;
                        }

                        builder.Add(new Construction
                        {
                            Id = constructionId,
                            Type = type,
                            Planet = planet,
                            Progress = progress,
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