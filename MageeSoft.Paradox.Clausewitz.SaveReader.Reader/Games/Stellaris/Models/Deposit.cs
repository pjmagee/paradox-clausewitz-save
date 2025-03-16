using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a deposit in the game state.
/// </summary>
public record Deposit
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Gets or sets whether the deposit is infinite.
    /// </summary>
    public required bool Infinite { get; init; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public Position? Position { get; init; }

    /// <summary>
    /// Default instance of Deposit.
    /// </summary>
    public static Deposit Default => new()
    {
        Type = string.Empty,
        Amount = 0,
        Infinite = false,
        Position = null
    };

    /// <summary>
    /// Loads all deposits from a game state root object.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of deposits.</returns>
    public static ImmutableArray<Deposit> Load(SaveObject root)
    {
        SaveObject? depositsObj;
        if (!root.TryGetSaveObject("deposits", out depositsObj))
        {
            return ImmutableArray<Deposit>.Empty;
        }

        var deposits = depositsObj.Properties
            .Select(kvp => kvp.Value)
            .OfType<SaveObject>()
            .Select(LoadSingle)
            .Where(x => x != null)
            .ToImmutableArray();

        return deposits!;
    }

    /// <summary>
    /// Loads a single deposit from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the deposit data.</param>
    /// <returns>A new Deposit instance.</returns>
    private static Deposit? LoadSingle(SaveObject obj)
    {
        string type;
        int amount;
        bool infinite;

        if (!obj.TryGetString("type", out type) ||
            !obj.TryGetInt("amount", out amount) ||
            !obj.TryGetBool("infinite", out infinite))
        {
            return null;
        }

        Position? position = null;

        if (obj.TryGetSaveObject("position", out var positionObj))
        {
            position = Position.Load(positionObj);
        }

        return new Deposit
        {
            Type = type,
            Amount = amount,
            Infinite = infinite,
            Position = position
        };
    }
} 






