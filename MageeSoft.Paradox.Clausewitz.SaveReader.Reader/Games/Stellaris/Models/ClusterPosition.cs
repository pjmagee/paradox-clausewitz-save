using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents the position of a cluster in the game state.
/// </summary>
public record ClusterPosition
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    [SaveName("x")]
    public required float X { get; init; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    [SaveName("y")]
    public required float Y { get; init; }

    /// <summary>
    /// Gets or sets the origin ID.
    /// </summary>
    [SaveName("origin")]
    public required long Origin { get; init; }

    /// <summary>
    /// Gets or sets whether the position is randomized.
    /// </summary>
    [SaveName("randomized")]
    public required bool Randomized { get; init; }

    /// <summary>
    /// Gets or sets the visual height.
    /// </summary>
    [SaveName("visual_height")]
    public required float VisualHeight { get; init; }

    /// <summary>
    /// Default instance of ClusterPosition.
    /// </summary>
    public static ClusterPosition Default => new()
    {
        X = 0f,
        Y = 0f,
        Origin = 0,
        Randomized = false,
        VisualHeight = 0f
    };

    /// <summary>
    /// Loads a cluster position from a SaveObject.
    /// </summary>
    /// <param name="obj">The SaveObject containing the cluster position data.</param>
    /// <returns>A new ClusterPosition instance if successful, null if any required property is missing.</returns>
    public static ClusterPosition? Load(SaveObject obj)
    {
        if (!obj.TryGetFloat("x", out var x) ||
            !obj.TryGetFloat("y", out var y) ||
            !obj.TryGetLong("origin", out var origin) ||
            !obj.TryGetBool("randomized", out var randomized) ||
            !obj.TryGetFloat("visual_height", out var visualHeight))
        {
            return null;
        }

        return new ClusterPosition
        {
            X = x,
            Y = y,
            Origin = origin,
            Randomized = randomized,
            VisualHeight = visualHeight
        };
    }
}