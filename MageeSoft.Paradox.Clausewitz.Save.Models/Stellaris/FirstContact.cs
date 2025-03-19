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
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the country involved in the first contact.
    /// </summary>
    public required long Country { get;set; }

    /// <summary>
    /// Gets or sets the target involved in the first contact.
    /// </summary>
    public required long Target { get;set; }

    /// <summary>
    /// Gets or sets the stage of the first contact.
    /// </summary>
    public required string Stage { get;set; }

    /// <summary>
    /// Gets or sets the type of the first contact.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the state of the first contact.
    /// </summary>
    public required string State { get;set; }

    /// <summary>
    /// Gets or sets the status of the first contact.
    /// </summary>
    public required string Status { get;set; }

    /// <summary>
    /// Gets or sets the progress of the first contact.
    /// </summary>
    public required float Progress { get;set; }

    /// <summary>
    /// Gets or sets the speed of the first contact.
    /// </summary>
    public required float Speed { get;set; }

    /// <summary>
    /// Gets or sets the cost of the first contact.
    /// </summary>
    public required float Cost { get;set; }

    /// <summary>
    /// Gets or sets the result of the first contact.
    /// </summary>
    public required string Result { get;set; }

    /// <summary>
    /// Gets or sets the event of the first contact.
    /// </summary>
    public required string Event { get;set; }

    /// <summary>
    /// Gets or sets the events associated with the first contact.
    /// </summary>
    public required ImmutableArray<FirstContactEvent> Events { get;set; }

    /// <summary>
    /// Gets or sets the scope of the first contact.
    /// </summary>
    public required FirstContactScope Scope { get;set; }
}






