using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a market order in the game state.
/// </summary>
public record MarketOrder
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required Owner Owner { get; init; }

    /// <summary>
    /// Gets or sets the resource.
    /// </summary>
    public required string Resource { get; init; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public required float Amount { get; init; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public required float Price { get; init; }

    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    /// Default instance of MarketOrder.
    /// </summary>
    public static MarketOrder Default => new()
    {
        Id = 0,
        Type = string.Empty,
        Owner = Owner.Default,
        Resource = string.Empty,
        Amount = 0f,
        Price = 0f,
        Date = string.Empty
    };

    /// <summary>
    /// Loads a market order from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the market order data.</param>
    /// <returns>A new MarketOrder instance.</returns>
    public static MarketOrder? Load(SaveObject saveObject)
    {
        long id;
        string type;
        Owner? owner;
        string resource;
        float amount;
        float price;
        string date;

        if (!saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetString("resource", out resource) ||
            !saveObject.TryGetFloat("amount", out amount) ||
            !saveObject.TryGetFloat("price", out price) ||
            !saveObject.TryGetString("date", out date))
        {
            return null;
        }

        SaveObject? ownerObj;
        if (!saveObject.TryGetSaveObject("owner", out ownerObj) || ownerObj == null ||
            (owner = Owner.Load(ownerObj)) == null)
        {
            return null;
        }

        return new MarketOrder
        {
            Id = id,
            Type = type,
            Owner = owner,
            Resource = resource,
            Amount = amount,
            Price = price,
            Date = date
        };
    }
} 






