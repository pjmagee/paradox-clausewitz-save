using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

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

}