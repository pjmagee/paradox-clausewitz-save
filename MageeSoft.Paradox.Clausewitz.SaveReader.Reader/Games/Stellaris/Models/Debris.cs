using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents debris in the game state.
/// </summary>
public record Debris
{
    /// <summary>
    /// Gets or sets the debris ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the country ID that owns the debris.
    /// </summary>
    public required long Owner { get; init; }

    /// <summary>
    /// Gets or sets the coordinate of the debris.
    /// </summary>
    public required Position Position { get; init; }

    /// <summary>
    /// Gets or sets the type of the debris.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets whether the debris is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets whether the debris is visible.
    /// </summary>
    public required bool IsVisible { get; init; }

    /// <summary>
    /// Gets or sets whether the debris is hostile.
    /// </summary>
    public required bool IsHostile { get; init; }

    /// <summary>
    /// Default instance of Debris.
    /// </summary>
    public static Debris Default => new()
    {
        Id = 0,
        Owner = 0,
        Position = Position.Default,
        Type = string.Empty,
        IsActive = false,
        IsVisible = false,
        IsHostile = false
    };

    /// <summary>
    /// Loads all debris from the game state.
    /// </summary>
    /// <param name="root">The game state root object to load from.</param>
    /// <returns>An immutable array of debris.</returns>
    public static ImmutableArray<Debris> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Debris>();

        if (!root.TryGetSaveObject("debris", out var debrisObj))
        {
            return builder.ToImmutable();
        }

        foreach (var debrisElement in debrisObj.Properties)
        {
            if (long.TryParse(debrisElement.Key, out var debrisId) && debrisElement.Value is SaveObject obj)
            {
                var debris = LoadSingle(obj, debrisId);
                if (debris != null)
                {
                    builder.Add(debris);
                }
            }
        }

        return builder.ToImmutable();
    }

    public static Debris? LoadSingle(SaveObject obj, long id)
    {
        SaveObject? positionObj;
        long ownerValue;
        string typeValue;
        bool isActiveValue;
        bool isVisibleValue;
        bool isHostileValue;

        if (!obj.TryGetSaveObject("position", out positionObj) ||
            !obj.TryGetLong("owner", out ownerValue) ||
            !obj.TryGetString("type", out typeValue) ||
            !obj.TryGetBool("is_active", out isActiveValue) ||
            !obj.TryGetBool("is_visible", out isVisibleValue) ||
            !obj.TryGetBool("is_hostile", out isHostileValue))
        {
            return null;
        }

        var position = positionObj != null ? Position.Load(positionObj) : null;
        if (position == null)
        {
            return null;
        }

        return new Debris
        {
            Id = id,
            Owner = ownerValue,
            Position = position,
            Type = typeValue,
            IsActive = isActiveValue,
            IsVisible = isVisibleValue,
            IsHostile = isHostileValue
        };
    }
} 






