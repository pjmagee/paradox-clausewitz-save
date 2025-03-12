using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using ValueType = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.ValueType;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an exhibit in the game state.
/// </summary>
public class Exhibit
{
    /// <summary>
    /// Gets or sets the exhibit ID.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets or sets the exhibit type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the planet ID where the exhibit is located.
    /// </summary>
    public int Planet { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the exhibit is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the exhibit state.
    /// </summary>
    public string ExhibitState { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the specimen information.
    /// </summary>
    public ExhibitSpecimen Specimen { get; init; } = new();

    /// <summary>
    /// Gets or sets the owner ID.
    /// </summary>
    public int Owner { get; init; }

    /// <summary>
    /// Loads all exhibits from the game save documents.
    /// </summary>
    /// <param name="documents">The game save documents to load from.</param>
    /// <returns>An immutable array of exhibits.</returns>
    public static ImmutableArray<Exhibit> Load(GameSaveDocuments documents)
    {
        var builder = ImmutableArray.CreateBuilder<Exhibit>();
        var exhibitsElement = (documents.GameState.Root as SaveObject)?.Properties
            .FirstOrDefault(p => p.Key == "exhibits");

        if (exhibitsElement.HasValue)
        {
            var exhibitsObj = exhibitsElement.Value.Value as SaveObject;
            if (exhibitsObj != null)
            {
                foreach (var exhibitElement in exhibitsObj.Properties)
                {
                    if (int.TryParse(exhibitElement.Key, out var exhibitId))
                    {
                        var obj = exhibitElement.Value as SaveObject;
                        if (obj == null)
                        {
                            continue;
                        }

                        var exhibit = new Exhibit
                        {
                            Id = exhibitId,
                            Type = GetScalarString(obj, "type"),
                            Planet = GetScalarInt(obj, "planet"),
                            IsActive = GetScalarBoolean(obj, "is_active"),
                            ExhibitState = GetScalarString(obj, "exhibit_state"),
                            Owner = GetScalarInt(obj, "owner"),
                            Specimen = LoadSpecimen(GetObject(obj, "specimen"))
                        };

                        builder.Add(exhibit);
                    }
                }
            }
        }

        return builder.ToImmutable();
    }

    static ExhibitSpecimen LoadSpecimen(SaveObject? specimenObj)
    {
        if (specimenObj == null)
        {
            return new ExhibitSpecimen();
        }

        var detailsArray = GetArray(specimenObj, "details_variables");
        var shortArray = GetArray(specimenObj, "short_variables");
        var nameArray = GetArray(specimenObj, "name_variables");

        var specimen = new ExhibitSpecimen
        {
            Specimen = GetScalarString(specimenObj, "specimen") ?? string.Empty,
            Origin = GetScalarString(specimenObj, "origin") ?? string.Empty,
            DateAdded = GetScalarString(specimenObj, "date_added") ?? string.Empty,
            DetailsVariables = detailsArray?.Items
                .Select(i => i.TryGetScalar<string>(out var value) ? value : string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? new List<string>(),
            ShortVariables = shortArray?.Items
                .Select(i => i.TryGetScalar<string>(out var value) ? value : string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? new List<string>(),
            NameVariables = nameArray?.Items
                .Select(i => i.TryGetScalar<string>(out var value) ? value : string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? new List<string>()
        };

        return specimen;
    }
}

/// <summary>
/// Represents specimen information for an exhibit.
/// </summary>
public class ExhibitSpecimen
{
    /// <summary>
    /// Gets or sets the specimen identifier.
    /// </summary>
    public string Specimen { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin of the specimen.
    /// </summary>
    public string Origin { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the specimen was added.
    /// </summary>
    public string DateAdded { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the details variables.
    /// </summary>
    public List<string> DetailsVariables { get; init; } = new();

    /// <summary>
    /// Gets or sets the short variables.
    /// </summary>
    public List<string> ShortVariables { get; init; } = new();

    /// <summary>
    /// Gets or sets the name variables.
    /// </summary>
    public List<string> NameVariables { get; init; } = new();
} 