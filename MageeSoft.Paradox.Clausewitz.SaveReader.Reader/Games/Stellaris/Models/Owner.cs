using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents an owner in the game state.
/// </summary>
public record Owner
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Default instance of Owner.
    /// </summary>
    public static Owner Default => new()
    {
        Type = string.Empty,
        Id = 0
    };

    /// <summary>
    /// Loads an owner from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the owner data.</param>
    /// <returns>A new Owner instance.</returns>
    public static Owner? Load(SaveObject saveObject)
    {
        string type;
        long id;

        if (!saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetLong("id", out id))
        {
            return null;
        }

        return new Owner
        {
            Type = type,
            Id = id
        };
    }
} 






