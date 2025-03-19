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
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the planet.
    /// </summary>
    public required int Planet { get;set; }

    /// <summary>
    /// Gets or sets whether the exhibit is active.
    /// </summary>
    public required bool IsActive { get;set; }

    /// <summary>
    /// Gets or sets the exhibit state.
    /// </summary>
    public required string ExhibitState { get;set; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required int Owner { get;set; }

    /// <summary>
    /// Gets or sets the specimen.
    /// </summary>
    public required ExhibitSpecimen Specimen { get;set; }

}