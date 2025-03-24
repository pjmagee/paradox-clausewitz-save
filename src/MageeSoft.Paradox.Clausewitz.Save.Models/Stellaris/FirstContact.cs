using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a first contact event in the game state.
/// </summary>  
[SaveModel]
public partial class FirstContact
{
    /// <summary>
    /// Gets or sets the ID of the first contact.
    /// </summary>
    public long? Id { get;set; }

    /// <summary>
    /// Gets or sets the country involved in the first contact.
    /// </summary>
    public long? Country { get;set; }

    /// <summary>
    /// Gets or sets the target involved in the first contact.
    /// </summary>
    public long? Target { get;set; }

    /// <summary>
    /// Gets or sets the stage of the first contact.
    /// </summary>
    public string? Stage { get;set; }

    /// <summary>
    /// Gets or sets the type of the first contact.
    /// </summary>
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets the state of the first contact.
    /// </summary>
    public string? State { get;set; }

    /// <summary>
    /// Gets or sets the status of the first contact.
    /// </summary>
    public string? Status { get;set; }

    /// <summary>
    /// Gets or sets the progress of the first contact.
    /// </summary>
    public float? Progress { get;set; }

    /// <summary>
    /// Gets or sets the speed of the first contact.
    /// </summary>
    public float? Speed { get;set; }

    /// <summary>
    /// Gets or sets the cost of the first contact.
    /// </summary>
    public float? Cost { get;set; }

    /// <summary>
    /// Gets or sets the result of the first contact.
    /// </summary>
    public string? Result { get;set; }

    /// <summary>
    /// Gets or sets the event of the first contact.
    /// </summary>
    public string? Event { get;set; }

    /// <summary>
    /// Gets or sets the events associated with the first contact.
    /// </summary>
    public List<FirstContactEvent>? Events { get;set; }

    /// <summary>
    /// Gets or sets the scope of the first contact.
    /// </summary>
    public FirstContactScope? Scope { get;set; }
}






