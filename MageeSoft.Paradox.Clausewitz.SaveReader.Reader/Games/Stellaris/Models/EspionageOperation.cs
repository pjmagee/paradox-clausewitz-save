using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record EspionageOperation
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Country { get; init; }
    public required int TargetCountry { get; init; }

    public static ImmutableArray<EspionageOperation> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<EspionageOperation>();
        var operationsElement = root.Properties.FirstOrDefault(p => p.Key == "espionage_operations");

        var operationsObj = operationsElement.Value as SaveObject;
        if (operationsObj != null)
        {
            foreach (var operationElement in operationsObj.Properties)
            {
                if (long.TryParse(operationElement.Key, out var operationId))
                {
                    var obj = operationElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var country = GetScalarInt(obj, "country");
                    var targetCountry = GetScalarInt(obj, "target_country");

                    if (type == null)
                    {
                        continue;
                    }

                    builder.Add(new EspionageOperation
                    {
                        Id = operationId,
                        Type = type,
                        Country = country,
                        TargetCountry = targetCountry
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 