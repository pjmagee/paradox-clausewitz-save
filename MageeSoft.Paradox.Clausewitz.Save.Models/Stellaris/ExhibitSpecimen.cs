using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents specimen information for an exhibit.
/// </summary>
public record ExhibitSpecimen
{
    /// <summary>
    /// Gets or sets the specimen identifier.
    /// </summary>
    public required string Specimen { get; init; }

    /// <summary>
    /// Gets or sets the origin of the specimen.
    /// </summary>
    public required string Origin { get; init; }

    /// <summary>
    /// Gets or sets the date the specimen was added.
    /// </summary>
    public required DateOnly DateAdded { get; init; }

    /// <summary>
    /// Gets or sets the details variables.
    /// </summary>
    public required ImmutableList<string> DetailsVariables { get; init; }

    /// <summary>
    /// Gets or sets the short variables.
    /// </summary>
    public required ImmutableList<string> ShortVariables { get; init; }

    /// <summary>
    /// Gets or sets the name variables.
    /// </summary>
    public required ImmutableList<string> NameVariables { get; init; }

    /// <summary>
    /// Gets the default instance of ExhibitSpecimen.
    /// </summary>
    public static ExhibitSpecimen Default { get; } = new()
    {
        Specimen = string.Empty,
        Origin = string.Empty,
        DateAdded = DateOnly.MinValue,
        DetailsVariables = ImmutableList<string>.Empty,
        ShortVariables = ImmutableList<string>.Empty,
        NameVariables = ImmutableList<string>.Empty
    };
}