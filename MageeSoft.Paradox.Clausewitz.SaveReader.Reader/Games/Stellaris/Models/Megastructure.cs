using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record Megastructure
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Owner { get; init; }
    public required Coordinate Coordinate { get; init; }
    public required float BuildProgress { get; init; }
    public required bool IsActive { get; init; }

    public static ImmutableArray<Megastructure> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Megastructure>();
        var megastructuresElement = root.Properties.FirstOrDefault(p => p.Key == "megastructures");

        var megastructuresObj = megastructuresElement.Value as SaveObject;
        if (megastructuresObj != null)
        {
            foreach (var megastructureElement in megastructuresObj.Properties)
            {
                if (long.TryParse(megastructureElement.Key, out var megastructureId))
                {
                    var obj = megastructureElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var owner = GetScalarInt(obj, "owner");
                    var coordinate = Coordinate.Load(GetObject(obj, "coordinate"));
                    var buildProgress = GetScalarFloat(obj, "build_progress");
                    var isActive = GetScalarBoolean(obj, "is_active");

                    if (type == null || coordinate == null)
                    {
                        continue;
                    }

                    builder.Add(new Megastructure
                    {
                        Id = megastructureId,
                        Type = type,
                        Owner = owner,
                        Coordinate = coordinate,
                        BuildProgress = buildProgress,
                        IsActive = isActive
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 