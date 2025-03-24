namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an exhibit in the game state.
/// </summary>
[SaveModel]
public partial class Exhibit
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets the planet.
    /// </summary>
    public int? Planet { get;set; }

    /// <summary>
    /// Gets or sets whether the exhibit is active.
    /// </summary>
    public bool? IsActive { get;set; }

    /// <summary>
    /// Gets or sets the exhibit state.
    /// </summary>
    public string? ExhibitState { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public int? Owner { get;set; }

    /// <summary>
    /// Gets or sets the specimen.
    /// </summary>
    public ExhibitSpecimen? Specimen { get;set; }

}