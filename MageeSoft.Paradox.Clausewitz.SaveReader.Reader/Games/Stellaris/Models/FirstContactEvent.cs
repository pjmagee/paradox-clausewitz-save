using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a first contact event.
/// </summary>
public class FirstContactEvent
{
    /// <summary>
    /// Gets or sets whether the event has expired.
    /// </summary>
    public bool Expired { get; set; }

    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event scope.
    /// </summary>
    public FirstContactScope Scope { get; set; } = new();

    /// <summary>
    /// Gets or sets the event picture.
    /// </summary>
    public string Picture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Loads a first contact event from a ClausewitzObject.
    /// </summary>
    /// <param name="element">The ClausewitzObject containing the event data.</param>
    /// <returns>A new FirstContactEvent instance.</returns>
    public static FirstContactEvent Load(SaveObject element)
    {
        var evt = new FirstContactEvent();

        foreach (var property in element.Properties)
        {
            switch (property.Key)
            {
                case "expired" when property.Value is Scalar<string> expiredScalar:
                    evt.Expired = expiredScalar.Value == "yes";
                    break;
                case "event_id" when property.Value is Scalar<string> eventIdScalar:
                    evt.EventId = eventIdScalar.RawText;
                    System.Console.WriteLine($"Found event_id: '{evt.EventId}' (raw text: '{eventIdScalar.RawText}', value: '{eventIdScalar.Value}')");
                    break;
                case "event_id":
                    System.Console.WriteLine($"Found event_id but type is {property.Value?.GetType().Name}");
                    if (property.Value.TryGetScalar<string>(out var eventIdValue))
                    {
                        evt.EventId = eventIdValue;
                        System.Console.WriteLine($"Successfully got event_id: '{evt.EventId}'");
                    }
                    break;
                case "scope" when property.Value is SaveObject scopeObj:
                    evt.Scope = FirstContactScope.Load(scopeObj);
                    break;
                case "picture" when property.Value is Scalar<string> pictureScalar:
                    evt.Picture = pictureScalar.RawText;
                    break;
                case "index" when property.Value is Scalar<int> indexScalar:
                    evt.Index = indexScalar.Value;
                    break;
            }
        }

        return evt;
    }
}