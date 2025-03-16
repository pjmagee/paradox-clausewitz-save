using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents a cluster in the game state.
/// </summary>
public record Cluster
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

    /// <summary>
    /// Default instance of Cluster.
    /// </summary>
    public static Cluster Default => new()
    {
        Id = 0,
        Position = ClusterPosition.Default,
        Radius = 0f,
        Objects = ImmutableArray<int>.Empty,
        Origin = null
    };

    /// <summary>
    /// Loads all clusters from the game state.
    /// </summary>
    /// <param name="root">The game state root object.</param>
    /// <returns>An immutable array of clusters.</returns>
    public static ImmutableArray<Cluster> Load(SaveObject root)
    {
        var builder = ImmutableArray.CreateBuilder<Cluster>();
        var clustersElement = root.Properties.FirstOrDefault(p => p.Key == "clusters");

        if (clustersElement.Value is not SaveArray clustersArray)
        {
            return ImmutableArray<Cluster>.Empty;
        }

        foreach (var item in clustersArray.Items)
        {
            if (item is not SaveObject obj)
            {
                continue;
            }

            var cluster = LoadSingle(obj);
            if (cluster != null)
            {
                builder.Add(cluster);
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Loads a single cluster from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the cluster data.</param>
    /// <returns>A new Cluster instance if successful, null if any required property is missing. Required properties are: id, position, and radius.</returns>
    private static Cluster? LoadSingle(SaveObject obj)
    {
        if (!obj.TryGetLong("id", out var id) ||
            !obj.TryGetSaveObject("position", out var positionObj) ||
            !obj.TryGetFloat("radius", out var radius))
        {
            return null;
        }

        var position = ClusterPosition.Load(positionObj) ?? ClusterPosition.Default;

        var objects = ImmutableArray<int>.Empty;
        if (obj.TryGetSaveArray("objects", out var objectsArray))
        {
            var objectsBuilder = ImmutableArray.CreateBuilder<int>();
            foreach (var element in objectsArray.Elements())
            {
                if (element is Scalar<int> scalar)
                {
                    objectsBuilder.Add(scalar.Value);
                }
            }
            objects = objectsBuilder.ToImmutable();
        }

        obj.TryGetInt("origin", out var origin);

        return new Cluster
        {
            Id = id,
            Position = position,
            Radius = radius,
            Objects = objects,
            Origin = origin
        };
    }
}