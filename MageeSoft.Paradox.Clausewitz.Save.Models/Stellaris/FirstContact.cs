using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a first contact event in the game state.
/// </summary>
public class FirstContact
{
    /// <summary>
    /// Gets or sets the ID of the first contact.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the country involved in the first contact.
    /// </summary>
    public required long Country { get; init; }

    /// <summary>
    /// Gets or sets the target involved in the first contact.
    /// </summary>
    public required long Target { get; init; }

    /// <summary>
    /// Gets or sets the stage of the first contact.
    /// </summary>
    public required string Stage { get; init; }

    /// <summary>
    /// Gets or sets the type of the first contact.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the state of the first contact.
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// Gets or sets the status of the first contact.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gets or sets the progress of the first contact.
    /// </summary>
    public required float Progress { get; init; }

    /// <summary>
    /// Gets or sets the speed of the first contact.
    /// </summary>
    public required float Speed { get; init; }

    /// <summary>
    /// Gets or sets the cost of the first contact.
    /// </summary>
    public required float Cost { get; init; }

    /// <summary>
    /// Gets or sets the result of the first contact.
    /// </summary>
    public required string Result { get; init; }

    /// <summary>
    /// Gets or sets the event of the first contact.
    /// </summary>
    public required string Event { get; init; }

    /// <summary>
    /// Gets or sets the events associated with the first contact.
    /// </summary>
    public required ImmutableArray<FirstContactEvent> Events { get; init; }

    /// <summary>
    /// Gets or sets the scope of the first contact.
    /// </summary>
    public required FirstContactScope Scope { get; init; }
}






