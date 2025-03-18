namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

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

}