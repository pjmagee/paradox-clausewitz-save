using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a movement path in the game state.
/// </summary>
public record MovementPath
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    /// Gets or sets the nodes.
    /// </summary>
    public required ImmutableArray<MovementPathNode> Nodes { get; init; }

    /// <summary>
    /// Creates a new instance of MovementPath with default values.
    /// </summary>
    public MovementPath()
    {
        Date = "0.01.01";
        Nodes = ImmutableArray<MovementPathNode>.Empty;
    }

    /// <summary>
    /// Default instance of MovementPath.
    /// </summary>
    public static MovementPath Default { get; } = new()
    {
        Date = "0.01.01",
        Nodes = ImmutableArray<MovementPathNode>.Empty
    };

    /// <summary>
    /// Loads a movement path from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the movement path data.</param>
    /// <returns>A new MovementPath instance.</returns>
    public static MovementPath? Load(SaveObject saveObject)
    {
        if (!saveObject.TryGetString("date", out var date))
        {
            return null;
        }

        SaveArray? nodeArray;
        var nodes = saveObject.TryGetSaveArray("node", out nodeArray) && nodeArray != null
            ? nodeArray.Elements()
                .OfType<SaveObject>()
                .Select(MovementPathNode.Load)
                .Where(x => x != null)
                .ToImmutableArray()
            : ImmutableArray<MovementPathNode>.Empty;

        return new MovementPath
        {
            Date = date,
            Nodes = nodes!
        };
    }
}