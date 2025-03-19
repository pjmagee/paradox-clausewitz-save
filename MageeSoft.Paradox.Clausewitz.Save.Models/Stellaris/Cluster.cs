using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a cluster in the game state.
/// </summary>  
[SaveModel]
public partial class Cluster
{
    /// <summary>
    /// Gets or sets the cluster ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the position of the cluster.
    /// </summary>
    public ClusterPosition Position { get;set; }

    /// <summary>
    /// Gets or sets the radius of the cluster.
    /// </summary>
    public float Radius { get;set; }

    /// <summary>
    /// Gets or sets the object IDs in the cluster.
    /// </summary>
    public ImmutableArray<int> Objects { get;set; } = ImmutableArray<int>.Empty;

    /// <summary>
    /// Gets or sets the origin ID of the cluster.
    /// </summary>
    public int? Origin { get;set; }
}