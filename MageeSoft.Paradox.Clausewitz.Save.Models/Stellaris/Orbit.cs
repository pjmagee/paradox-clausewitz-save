namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an orbit in the game state.
/// </summary>
public record Orbit
{
    /// <summary>
    /// Gets or sets the orbitable.
    /// </summary>
    public required Orbitable Orbitable { get; init; }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public required int Index { get; init; }
}






