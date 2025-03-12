using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public record ArchaeologicalSite
{
    public required long Id { get; init; }
    public required string Type { get; init; }
    public required int Planet { get; init; }
    public required int Chapter { get; init; }
    public required int Stage { get; init; }
    public required bool IsActive { get; init; }

    public static ImmutableArray<ArchaeologicalSite> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<ArchaeologicalSite>();
        KeyValuePair<string, SaveElement> sitesElement = root.Properties.FirstOrDefault(p => p.Key == "archaeological_sites");

        var sitesObj = sitesElement.Value as SaveObject;
        
        if (sitesObj != null)
        {
            foreach (var siteElement in sitesObj.Properties)
            {
                if (long.TryParse(siteElement.Key, out var siteId))
                {
                    var obj = siteElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var type = GetScalarString(obj, "type");
                    var planet = GetScalarInt(obj, "planet");
                    var chapter = GetScalarInt(obj, "chapter");
                    var stage = GetScalarInt(obj, "stage");
                    var isActive = GetScalarBoolean(obj, "is_active");

                    if (type == null)
                    {
                        continue;
                    }

                    builder.Add(new ArchaeologicalSite
                    {
                        Id = siteId,
                        Type = type,
                        Planet = planet,
                        Chapter = chapter,
                        Stage = stage,
                        IsActive = isActive
                    });
                }
            }
        }

        return builder.ToImmutable();
    }
} 