using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a pop faction in the game state.
/// </summary>
public record PopFaction
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the support.
    /// </summary>
    public required float Support { get; init; }

    /// <summary>
    /// Gets or sets the approval.
    /// </summary>
    public required float Approval { get; init; }

    /// <summary>
    /// Default instance of PopFaction.
    /// </summary>
    public static PopFaction Default => new()
    {
        Id = 0,
        Name = string.Empty,
        Type = string.Empty,
        Support = 0f,
        Approval = 0f
    };

    /// <summary>
    /// Loads a pop faction from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the pop faction data.</param>
    /// <returns>A new PopFaction instance.</returns>
    public static PopFaction? Load(SaveObject saveObject)
    {
        long id;
        string name;
        string type;
        float support;
        float approval;

        if (!saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetString("name", out name) ||
            !saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetFloat("support", out support) ||
            !saveObject.TryGetFloat("approval", out approval))
        {
            return null;
        }

        return new PopFaction
        {
            Id = id,
            Name = name,
            Type = type,
            Support = support,
            Approval = approval
        };
    }
}







