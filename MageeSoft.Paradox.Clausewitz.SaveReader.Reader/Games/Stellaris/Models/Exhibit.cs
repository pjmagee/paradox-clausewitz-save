using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an exhibit in the game state.
/// </summary>
public record Exhibit
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the planet.
    /// </summary>
    public required int Planet { get; init; }

    /// <summary>
    /// Gets or sets whether the exhibit is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the exhibit state.
    /// </summary>
    public required string ExhibitState { get; init; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required int Owner { get; init; }

    /// <summary>
    /// Gets or sets the specimen.
    /// </summary>
    public required ExhibitSpecimen Specimen { get; init; }

    /// <summary>
    /// Default instance of Exhibit.
    /// </summary>
    public static Exhibit Default => new()
    {
        Type = string.Empty,
        Planet = 0,
        IsActive = false,
        ExhibitState = string.Empty,
        Owner = 0,
        Specimen = ExhibitSpecimen.Default
    };

    /// <summary>
    /// Loads all exhibits from a game state root object.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of exhibits.</returns>
    public static ImmutableArray<Exhibit> Load(SaveObject root)
    {
        if (!root.TryGetSaveObject("exhibits", out var exhibitsObj))
        {
            return ImmutableArray<Exhibit>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<Exhibit>();
        foreach (var (_, value) in exhibitsObj.Properties)
        {
            if (value is SaveObject obj)
            {
                var exhibit = LoadSingle(obj);
                if (exhibit != null)
                {
                    builder.Add(exhibit);
                }
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Loads a single exhibit from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the exhibit data.</param>
    /// <returns>A new Exhibit instance.</returns>
    private static Exhibit? LoadSingle(SaveObject obj)
    {
        string type;
        int planet;
        bool isActive;
        string exhibitState;
        int owner;

        if (!obj.TryGetString("type", out type) ||
            !obj.TryGetInt("planet", out planet) ||
            !obj.TryGetBool("is_active", out isActive) ||
            !obj.TryGetString("exhibit_state", out exhibitState) ||
            !obj.TryGetInt("owner", out owner))
        {
            return null;
        }

        SaveObject? specimenObj;
        var specimen = obj.TryGetSaveObject("specimen", out specimenObj) && specimenObj != null
            ? LoadSpecimen(specimenObj)
            : ExhibitSpecimen.Default;

        return new Exhibit
        {
            Type = type,
            Planet = planet,
            IsActive = isActive,
            ExhibitState = exhibitState,
            Owner = owner,
            Specimen = specimen
        };
    }

    /// <summary>
    /// Loads a specimen from a SaveObject.
    /// </summary>
    /// <param name="specimenObj">The SaveObject containing the specimen data.</param>
    /// <returns>A new ExhibitSpecimen instance.</returns>
    private static ExhibitSpecimen LoadSpecimen(SaveObject specimenObj)
    {
        if (!specimenObj.TryGetString("specimen", out var specimen) ||
            !specimenObj.TryGetString("origin", out var origin) ||
            !specimenObj.TryGetString("date_added", out var dateAdded))
        {
            return ExhibitSpecimen.Default;
        }

        var detailsVariables = ImmutableList<string>.Empty;
        var shortVariables = ImmutableList<string>.Empty;
        var nameVariables = ImmutableList<string>.Empty;

        if (specimenObj.TryGetSaveArray("details_variables", out var detailsArray))
        {
            detailsVariables = detailsArray.Elements()
                .OfType<Scalar<string>>()
                .Select(s => s.Value)
                .ToImmutableList();
        }

        if (specimenObj.TryGetSaveArray("short_variables", out var shortArray))
        {
            shortVariables = shortArray.Elements()
                .OfType<Scalar<string>>()
                .Select(s => s.Value)
                .ToImmutableList();
        }

        if (specimenObj.TryGetSaveArray("name_variables", out var nameArray))
        {
            nameVariables = nameArray.Elements()
                .OfType<Scalar<string>>()
                .Select(s => s.Value)
                .ToImmutableList();
        }

        return new ExhibitSpecimen
        {
            Specimen = specimen,
            Origin = origin,
            DateAdded = DateOnly.Parse(dateAdded),
            DetailsVariables = detailsVariables,
            ShortVariables = shortVariables,
            NameVariables = nameVariables
        };
    }
}