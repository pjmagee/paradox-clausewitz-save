using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

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

    /// <summary>
    /// Loads a first contact event from a ClausewitzObject.
    /// </summary>
    /// <param name="element">The ClausewitzObject containing the event data.</param>
    /// <returns>A new FirstContactEvent instance.</returns>
    public static FirstContactEvent? Load(SaveObject element)
    {
        bool expired = false;
        string eventId = string.Empty;
        FirstContactScope? scope = null;
        string picture = string.Empty;
        int index = 0;

        if (element.TryGetString("expired", out var expiredValue))
        {
            expired = expiredValue == "yes";
        }

        if (element.TryGetString("event_id", out var eventIdValue))
        {
            eventId = eventIdValue;
        }

        if (element.TryGetSaveObject("scope", out var scopeObj))
        {
            scope = FirstContactScope.Load(scopeObj);
        }

        if (element.TryGetString("picture", out var pictureValue))
        {
            picture = pictureValue;
        }

        if (element.TryGetInt("index", out var indexValue))
        {
            index = indexValue;
        }

        if (scope == null)
        {
            scope = new FirstContactScope
            {
                Type = string.Empty,
                Id = 0,
                OpenerId = 0,
                RandomAllowed = false,
                Random = 0f,
                Root = 0,
                From = 0,
                Systems = ImmutableArray<long>.Empty,
                Planets = ImmutableArray<long>.Empty,
                Fleets = ImmutableArray<long>.Empty
            };
        }

        return new FirstContactEvent
        {
            Expired = expired,
            EventId = eventId,
            Scope = scope,
            Picture = picture,
            Index = index
        };
    }
}






