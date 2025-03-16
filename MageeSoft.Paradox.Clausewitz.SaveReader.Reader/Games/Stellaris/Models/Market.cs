using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a market in the game state.
/// </summary>
public record Market
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
    /// Gets or sets the resources.
    /// </summary>
    public required ImmutableArray<MarketResource> Resources { get; init; }

    /// <summary>
    /// Gets or sets the orders.
    /// </summary>
    public required ImmutableArray<MarketOrder> Orders { get; init; }

    /// <summary>
    /// Gets or sets the history.
    /// </summary>
    public required ImmutableArray<MarketHistory> History { get; init; }

    /// <summary>
    /// Default instance of Market.
    /// </summary>
    public static Market Default => new()
    {
        Id = 0,
        Type = string.Empty,
        Owner = Owner.Default,
        Resources = ImmutableArray<MarketResource>.Empty,
        Orders = ImmutableArray<MarketOrder>.Empty,
        History = ImmutableArray<MarketHistory>.Empty
    };

    /// <summary>
    /// Loads a market from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the market data.</param>
    /// <returns>A new Market instance.</returns>
    public static Market? Load(SaveObject saveObject)
    {
        long id;
        string type;
        Owner? owner;

        if (!saveObject.TryGetLong("id", out id) ||
            !saveObject.TryGetString("type", out type))
        {
            return null;
        }

        SaveObject? ownerObj;
        if (!saveObject.TryGetSaveObject("owner", out ownerObj) || ownerObj == null ||
            (owner = Owner.Load(ownerObj)) == null)
        {
            return null;
        }

        SaveObject? resourcesObj;
        if (!saveObject.TryGetSaveObject("resources", out resourcesObj))
        {
            return null;
        }

        var resources = resourcesObj?.Properties
            .Select(kvp => kvp.Value)
            .OfType<SaveObject>()
            .Select(MarketResource.Load)
            .Where(resource => resource != null)
            .ToImmutableArray() ?? ImmutableArray<MarketResource>.Empty;

        SaveObject? ordersObj;
        if (!saveObject.TryGetSaveObject("orders", out ordersObj))
        {
            return null;
        }

        var orders = ordersObj?.Properties
            .Select(kvp => kvp.Value)
            .OfType<SaveObject>()
            .Select(MarketOrder.Load)
            .Where(order => order != null)
            .ToImmutableArray() ?? ImmutableArray<MarketOrder>.Empty;

        SaveObject? historyObj;
        if (!saveObject.TryGetSaveObject("history", out historyObj))
        {
            return null;
        }

        var history = historyObj?.Properties
            .Select(kvp => kvp.Value)
            .OfType<SaveObject>()
            .Select(MarketHistory.Load)
            .Where(history => history != null)
            .ToImmutableArray() ?? ImmutableArray<MarketHistory>.Empty;

        return new Market
        {
            Id = id,
            Type = type,
            Owner = owner,
            Resources = resources!,
            Orders = orders!,
            History = history!
        };
    }
} 






