using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a market resource in the game state.
/// </summary>
public record MarketResource
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public required float Price { get; init; }

    /// <summary>
    /// Gets or sets the demand.
    /// </summary>
    public required float Demand { get; init; }

    /// <summary>
    /// Gets or sets the supply.
    /// </summary>
    public required float Supply { get; init; }

    /// <summary>
    /// Default instance of MarketResource.
    /// </summary>
    public static MarketResource Default => new()
    {
        Type = string.Empty,
        Price = 0f,
        Demand = 0f,
        Supply = 0f
    };

    /// <summary>
    /// Loads a market resource from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the market resource data.</param>
    /// <returns>A new MarketResource instance.</returns>
    public static MarketResource? Load(SaveObject saveObject)
    {
        string type;
        float price;
        float demand;
        float supply;

        if (!saveObject.TryGetString("type", out type) ||
            !saveObject.TryGetFloat("price", out price) ||
            !saveObject.TryGetFloat("demand", out demand) ||
            !saveObject.TryGetFloat("supply", out supply))
        {
            return null;
        }

        return new MarketResource
        {
            Type = type,
            Price = price,
            Demand = demand,
            Supply = supply
        };
    }
} 






