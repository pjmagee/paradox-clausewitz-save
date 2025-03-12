using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;
using System.Collections.Immutable;
using static MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models.SaveObjectHelper;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a first contact event in the game state.
/// </summary>
public class FirstContacts
{
    /// <summary>
    /// Gets or sets the owner of the first contact.
    /// </summary>
    public long Owner { get; set; }

    /// <summary>
    /// Gets or sets the country involved in the first contact.
    /// </summary>
    public long Country { get; set; }

    /// <summary>
    /// Gets or sets the location of the first contact.
    /// </summary>
    public long Location { get; set; }

    /// <summary>
    /// Gets or sets the leader involved in the first contact.
    /// </summary>
    public long Leader { get; set; }

    /// <summary>
    /// Gets or sets the date of the first contact.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stage of the first contact.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event ID associated with the first contact.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the first contact.
    /// </summary>
    public LocalizedText Name { get; set; } = new();

    /// <summary>
    /// Gets or sets the last roll value.
    /// </summary>
    public float LastRoll { get; set; }

    /// <summary>
    /// Gets or sets the number of days left.
    /// </summary>
    public float DaysLeft { get; set; }

    /// <summary>
    /// Gets or sets the difficulty level.
    /// </summary>
    public float Difficulty { get; set; }

    /// <summary>
    /// Gets or sets the number of clues.
    /// </summary>
    public float Clues { get; set; }

    /// <summary>
    /// Gets or sets the status of the first contact.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event details.
    /// </summary>
    public FirstContactEvent Event { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of events.
    /// </summary>
    public List<FirstContactEvent> Events { get; set; } = new();

    /// <summary>
    /// Gets or sets the flags associated with the first contact.
    /// </summary>
    public Dictionary<string, long> Flags { get; set; } = new();

    /// <summary>
    /// Gets or sets the completed stages.
    /// </summary>
    public List<CompletedStage> Completed { get; set; } = new();

    /// <summary>
    /// Loads all first contacts from the game state.
    /// </summary>
    /// <param name="root">The game state root object to load from.</param>
    /// <returns>An immutable array of first contacts.</returns>
    public static ImmutableArray<FirstContacts> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<FirstContacts>();
        var contactsElement = root.Properties
            .FirstOrDefault(p => p.Key == "first_contacts");

        var contactsObj = contactsElement.Value as SaveObject;
        if (contactsObj != null)
        {
            foreach (var contactElement in contactsObj.Properties)
            {
                if (long.TryParse(contactElement.Key, out var contactId))
                {
                    var obj = contactElement.Value as SaveObject;
                    if (obj == null)
                    {
                        continue;
                    }

                    var contact = new FirstContacts
                    {
                        Owner = GetScalarLong(obj, "owner"),
                        Country = GetScalarLong(obj, "country"),
                        Location = GetScalarLong(obj, "location"),
                        Leader = GetScalarLong(obj, "leader"),
                        Date = GetScalarString(obj, "date") ?? string.Empty,
                        Stage = GetScalarString(obj, "stage") ?? string.Empty,
                        EventId = GetScalarString(obj, "event_id") ?? string.Empty,
                        Name = GetObject(obj, "name") is SaveObject nameObj ? LocalizedText.Load(nameObj) : new(),
                        LastRoll = GetScalarFloat(obj, "last_roll"),
                        DaysLeft = GetScalarFloat(obj, "days_left"),
                        Difficulty = GetScalarFloat(obj, "difficulty"),
                        Clues = GetScalarFloat(obj, "clues"),
                        Status = GetScalarString(obj, "status") ?? string.Empty,
                        Event = GetObject(obj, "event") is SaveObject eventObj ? LoadEvent(eventObj) : new(),
                        Events = GetArray(obj, "events")?.Items
                            .OfType<SaveObject>()
                            .Select(LoadEvent)
                            .ToList() ?? new(),
                        Flags = GetObject(obj, "flags") is SaveObject flagsObj ? LoadFlags(flagsObj) : new(),
                        Completed = GetArray(obj, "completed")?.Items
                            .OfType<SaveObject>()
                            .Select(LoadCompletedStage)
                            .ToList() ?? new()
                    };

                    builder.Add(contact);
                }
            }
        }

        return builder.ToImmutable();
    }

    private static FirstContactEvent LoadEvent(SaveObject eventObj)
    {
        return new FirstContactEvent
        {
            Scope = GetObject(eventObj, "scope") is SaveObject scopeObj ? FirstContactScope.Load(scopeObj) : new(),
            //Effect = GetScalarString(eventObj, "effect") ?? string.Empty,
            Picture = GetScalarString(eventObj, "picture") ?? string.Empty,
            Index = GetScalarInt(eventObj, "index")
        };
    }

    private static CompletedStage LoadCompletedStage(SaveObject stageObj)
    {
        return new CompletedStage
        {
            Stage = GetScalarString(stageObj, "stage") ?? string.Empty,
            Date = GetScalarString(stageObj, "date") ?? string.Empty
        };
    }

    private static Dictionary<string, long> LoadFlags(SaveObject flagsObj)
    {
        var flags = new Dictionary<string, long>();
        foreach (var flag in flagsObj.Properties)
        {
            if (flag.Value?.TryGetScalar<long>(out var value) == true)
            {
                flags[flag.Key] = value;
            }
        }
        return flags;
    }
}