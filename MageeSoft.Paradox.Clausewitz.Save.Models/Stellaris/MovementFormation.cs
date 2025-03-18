namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a movement formation in the game state.
/// </summary>
public record MovementFormation
{
    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    public required float Scale { get; init; }

    /// <summary>
    /// Gets or sets the angle.
    /// </summary>
    public required float Angle { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }
}






