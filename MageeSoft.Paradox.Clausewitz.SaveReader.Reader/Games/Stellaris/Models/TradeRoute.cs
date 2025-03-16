using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a trade route in the game state.
/// </summary>
public record TradeRoute
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    public required Owner Owner { get; init; }

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    public required ImmutableArray<Position> Path { get; init; }

    /// <summary>
    /// Default instance of TradeRoute.
    /// </summary>
    public static TradeRoute Default => new()
    {
        Id = 0,
        Owner = Owner.Default,
        Path = ImmutableArray<Position>.Empty
    };

    /// <summary>
    /// Loads a trade route from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the trade route data.</param>
    /// <returns>A new TradeRoute instance.</returns>
    public static TradeRoute? Load(SaveObject saveObject)
    {
        if (!saveObject.TryGetLong("id", out var id))
        {
            return null;
        }

        if (!saveObject.TryGetSaveObject("owner", out var ownerObj))
        {
            return null;
        }

        var owner = Owner.Load(ownerObj);
        if (owner == null)
        {
            return null;
        }

        var path = ImmutableArray<Position>.Empty;
        if (saveObject.TryGetSaveArray("path", out var pathArray))
        {
            var builder = ImmutableArray.CreateBuilder<Position>();
            foreach (var element in pathArray.Elements())
            {
                if (element is SaveObject obj)
                {
                    var position = Position.Load(obj);
                    if (position != null)
                    {
                        builder.Add(position);
                    }
                }
            }
            path = builder.ToImmutable();
        }

        return new TradeRoute
        {
            Id = id,
            Owner = owner,
            Path = path
        };
    }
} 






