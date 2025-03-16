using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;
using SaveArray = MageeSoft.Paradox.Clausewitz.SaveReader.Parser.SaveArray;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a storm in the game state.
/// </summary>
public record Storm
{
    /// <summary>
    /// Gets or sets the storm ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the storm.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the position of the storm.
    /// </summary>
    public required Position Position { get; init; }

    /// <summary>
    /// Default instance of Storm.
    /// </summary>
    public static Storm Default => new()
    {
        Id = 0,
        Type = string.Empty,
        Position = Position.Default
    };

    /// <summary>
    /// Loads all storms from the game state.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of storms.</returns>
    public static ImmutableArray<Storm> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Storm>();

        if (!root.TryGetSaveObject("storms", out var stormsObj))
        {
            return builder.ToImmutable();
        }

        foreach (var (key, value) in stormsObj.Properties)
        {
            if (long.TryParse(key, out var id) && value is SaveObject obj)
            {
                var storm = LoadSingle(id, obj);
                if (storm != null)
                {
                    builder.Add(storm);
                }
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Loads a single storm from a SaveObject.
    /// </summary>
    /// <param name="id">The storm ID.</param>
    /// <param name="obj">The SaveObject containing the storm data.</param>
    /// <returns>A new Storm instance if successful, null if any required property is missing. Required properties are: type and position.</returns>
    private static Storm? LoadSingle(long id, SaveObject obj)
    {
        if (!obj.TryGetString("type", out var type) ||
            !obj.TryGetSaveObject("position", out var positionObj))
        {
            return null;
        }

        var position = Position.Load(positionObj) ?? Position.Default;

        return new Storm
        {
            Id = id,
            Type = type,
            Position = position
        };
    }
} 






