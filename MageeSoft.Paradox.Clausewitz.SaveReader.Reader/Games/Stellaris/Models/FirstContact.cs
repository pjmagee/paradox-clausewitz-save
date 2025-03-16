using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a first contact event in the game state.
/// </summary>
public record FirstContact
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

    /// <summary>
    /// Default instance of FirstContact.
    /// </summary>
    public static FirstContact Default => new()
    {
        Id = 0,
        Country = 0,
        Target = 0,
        Stage = string.Empty,
        Type = string.Empty,
        State = string.Empty,
        Status = string.Empty,
        Progress = 0,
        Speed = 0,
        Cost = 0,
        Result = string.Empty,
        Event = string.Empty,
        Events = ImmutableArray<FirstContactEvent>.Empty,
        Scope = FirstContactScope.Default
    };

    /// <summary>
    /// Loads all first contacts from the game state.
    /// </summary>
    /// <param name="root">The game state root object to load from.</param>
    /// <returns>An immutable array of first contacts.</returns>
    public static ImmutableArray<FirstContact> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<FirstContact>();

        if (!root.TryGetSaveObject("first_contacts", out var firstContactsObj) || firstContactsObj == null)
        {
            return builder.ToImmutable();
        }

        foreach (var firstContactElement in firstContactsObj.Properties)
        {
            if (long.TryParse(firstContactElement.Key, out var firstContactId) && firstContactElement.Value is SaveObject obj)
            {
                var firstContact = LoadSingle(obj);
                if (firstContact != null)
                {
                    builder.Add(firstContact with { Id = firstContactId });
                }
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Loads a single first contact from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the first contact data.</param>
    /// <returns>A new FirstContact instance if successful, null if any required property is missing. Required properties are: country, target, stage, type, state, status, progress, speed, cost, result, event, events, and scope.</returns>
    private static FirstContact? LoadSingle(SaveObject obj)
    {
        if (!obj.TryGetLong("country", out var country) ||
            !obj.TryGetLong("target", out var target) ||
            !obj.TryGetString("stage", out var stage) ||
            !obj.TryGetString("type", out var type) ||
            !obj.TryGetString("state", out var state) ||
            !obj.TryGetString("status", out var status) ||
            !obj.TryGetFloat("progress", out var progress) ||
            !obj.TryGetFloat("speed", out var speed) ||
            !obj.TryGetFloat("cost", out var cost) ||
            !obj.TryGetString("result", out var result) ||
            !obj.TryGetString("event", out var eventName))
        {
            return null;
        }

        var scope = obj.TryGetSaveObject("scope", out var scopeObj) && scopeObj != null
            ? FirstContactScope.Load(scopeObj) ?? FirstContactScope.Default
            : FirstContactScope.Default;

        var events = obj.TryGetSaveArray("events", out var eventsArray) && eventsArray != null
            ? eventsArray.Elements()
                .OfType<SaveObject>()
                .Select(FirstContactEvent.Load)
                .Where(e => e != null)
                .ToImmutableArray()
            : ImmutableArray<FirstContactEvent>.Empty;

        return new FirstContact
        {
            Id = 0, // This will be set by the caller
            Country = country,
            Target = target,
            Stage = stage,
            Type = type,
            State = state,
            Status = status,
            Progress = progress,
            Speed = speed,
            Cost = cost,
            Result = result,
            Event = eventName,
            Events = events,
            Scope = scope
        };
    }
}






