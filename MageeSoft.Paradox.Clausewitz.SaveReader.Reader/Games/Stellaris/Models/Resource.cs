using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a resource in the game state.
/// </summary>
public record Resource
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public required float Amount { get; init; }

    /// <summary>
    /// Default instance of Resource.
    /// </summary>
    public static Resource Default => new()
    {
        Type = string.Empty,
        Amount = 0f
    };

    /// <summary>
    /// Creates a new instance of Resource with default values.
    /// </summary>
    public Resource()
    {
        Type = string.Empty;
        Amount = 0f;
    }

    /// <summary>
    /// Creates a new instance of Resource with specified values.
    /// </summary>
    public Resource(string type, float amount)
    {
        Type = type ?? string.Empty;
        Amount = amount;
    }

    /// <summary>
    /// Loads a resource from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the resource data.</param>
    /// <returns>A new Resource instance.</returns>
    public static Resource? Load(SaveObject saveObject)
    {
        string type;
        float amount;

        if (!saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetFloat("amount", out amount))
        {
            return null;
        }

        return new Resource
        {
            Type = type,
            Amount = amount
        };
    }
} 






