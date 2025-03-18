using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a cluster in the game state.
/// </summary>
public class Cluster
{
    /// <summary>
    /// Gets or sets the cluster ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the position of the cluster.
    /// </summary>
    public required ClusterPosition Position { get; init; }

    /// <summary>
    /// Gets or sets the radius of the cluster.
    /// </summary>
    public required float Radius { get; init; }

    /// <summary>
    /// Gets or sets the object IDs in the cluster.
    /// </summary>
    public ImmutableArray<int> Objects { get; init; } = ImmutableArray<int>.Empty;

    /// <summary>
    /// Gets or sets the origin ID of the cluster.
    /// </summary>
    public int? Origin { get; init; }
}