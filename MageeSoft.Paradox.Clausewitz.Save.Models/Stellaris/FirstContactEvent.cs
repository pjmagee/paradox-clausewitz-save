namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a first contact event.
/// </summary>
public record FirstContactEvent
{
    /// <summary>
    /// Gets or sets whether the event has expired.
    /// </summary>
    public bool Expired { get; init; }

    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string EventId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the event scope.
    /// </summary>
    public required FirstContactScope Scope { get; init; }

    /// <summary>
    /// Gets or sets the event picture.
    /// </summary>
    public string Picture { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the event index.
    /// </summary>
    public int Index { get; init; }
}






