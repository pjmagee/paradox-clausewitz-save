using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents specimen information for an exhibit.
/// </summary>
[SaveModel]
public partial class ExhibitSpecimen
{
    /// <summary>
    /// Gets or sets the specimen identifier.
    /// </summary>
    public string Specimen { get;set; }

    /// <summary>
    /// Gets or sets the origin of the specimen.
    /// </summary>
    public string Origin { get;set; }

    /// <summary>
    /// Gets or sets the date the specimen was added.
    /// </summary>
    public DateOnly DateAdded { get;set; }

    /// <summary>
    /// Gets or sets the details variables.
    /// </summary>
    public ImmutableList<string> DetailsVariables { get;set; }

    /// <summary>
    /// Gets or sets the short variables.
    /// </summary>
    public ImmutableList<string> ShortVariables { get;set; }

    /// <summary>
    /// Gets or sets the name variables.
    /// </summary>
    public ImmutableList<string> NameVariables { get;set; }
}