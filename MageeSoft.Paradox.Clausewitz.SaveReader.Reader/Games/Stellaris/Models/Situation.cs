using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Situation
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Country { get; init; }
    public required float Progress { get; init; }
    public required bool IsActive { get; init; }
    public required ImmutableDictionary<string, float> Modifiers { get; init; }

    public static ImmutableArray<Situation> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Situation>();
        var situationsElement = root.Properties
            .FirstOrDefault(p => p.Key == "situations");

        var situationsObj = situationsElement.Value as SaveObject;
        if (situationsObj != null)
        {
            foreach (var situationElement in situationsObj.Properties)
            {
                if (long.TryParse(situationElement.Key, out var situationId))
                {
                    var obj = situationElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var country = GetScalarInt(obj, "country");
                    var progress = GetScalarFloat(obj, "progress");
                    var isActive = GetScalarBoolean(obj, "is_active");
                        
                    var modifiers = GetObject(obj, "modifiers")?.Properties.ToImmutableDictionary(
                        kvp => kvp.Key,
                        kvp => (kvp.Value as Scalar<float>)?.Value ?? 0
                    ) ?? ImmutableDictionary<string, float>.Empty;

                    if (type == null)
                    {
                        continue;
                    }

                    builder.Add(new Situation
                    {
                        Id = situationId,
                        Type = type,
                        Country = country,
                        Progress = progress,
                        IsActive = isActive,
                        Modifiers = modifiers
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 