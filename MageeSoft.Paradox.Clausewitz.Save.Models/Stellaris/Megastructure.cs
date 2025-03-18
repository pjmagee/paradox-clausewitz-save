namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a megastructure in the game state.
/// </summary>
public record Megastructure
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get; init; }

    /// <summary>
    /// Gets or sets the stage.
    /// </summary>
    public string Stage { get; init; }

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public Coordinate Coordinate { get; init; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public float Progress { get; init; }

    /// <summary>
    /// Gets or sets whether the megastructure is active.
    /// </summary>
    public bool IsActive { get; init; }
}