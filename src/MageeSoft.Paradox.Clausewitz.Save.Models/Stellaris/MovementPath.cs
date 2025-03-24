using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a movement path in the game state.
/// </summary>
[SaveModel]
public partial class MovementPath
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public string? Date { get;set; }

    /// <summary>
    /// Gets or sets the nodes.
    /// </summary>
    public List<MovementPathNode>? Nodes { get;set; }

}