namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a first contact event.
/// </summary>
[SaveModel]
public partial class FirstContactEvent
{
    /// <summary>
    /// Gets or sets whether the event has expired.
    /// </summary>
    public bool Expired { get;set; }

    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string EventId { get;set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event scope.
    /// </summary>
    public required FirstContactScope Scope { get;set; }

    /// <summary>
    /// Gets or sets the event picture.
    /// </summary>
    public string Picture { get;set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event index.
    /// </summary>
    public int Index { get;set; }
}






