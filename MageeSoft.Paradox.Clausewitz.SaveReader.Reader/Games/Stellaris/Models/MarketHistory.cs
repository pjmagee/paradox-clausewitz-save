using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents market history in the game state.
/// </summary>
public record MarketHistory
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public required ImmutableArray<MarketResource> Resources { get; init; }

    /// <summary>
    /// Default instance of MarketHistory.
    /// </summary>
    public static MarketHistory Default => new()
    {
        Date = string.Empty,
        Resources = ImmutableArray<MarketResource>.Empty
    };

    /// <summary>
    /// Loads market history from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the market history data.</param>
    /// <returns>A new MarketHistory instance.</returns>
    public static MarketHistory? Load(SaveObject saveObject)
    {
        string date;
        if (!saveObject.TryGetString("date", out date))
        {
            return null;
        }

        var resources = ImmutableArray<MarketResource>.Empty;
        if (saveObject.TryGetSaveArray("resources", out var resourcesArray))
        {
            var builder = ImmutableArray.CreateBuilder<MarketResource>();
            foreach (var element in resourcesArray.Elements())
            {
                if (element is SaveObject obj)
                {
                    var resource = MarketResource.Load(obj);
                    if (resource != null)
                    {
                        builder.Add(resource);
                    }
                }
            }
            resources = builder.ToImmutable();
        }

        return new MarketHistory
        {
            Date = date,
            Resources = resources
        };
    }
} 






