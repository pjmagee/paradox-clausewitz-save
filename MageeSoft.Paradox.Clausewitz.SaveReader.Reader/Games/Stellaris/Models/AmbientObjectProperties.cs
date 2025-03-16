using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents the properties of an ambient object.
/// </summary>
public class AmbientObjectProperties
{
    /// <summary>
    /// Gets or sets the coordinate of the properties.
    /// </summary>
    public required Coordinate Coordinate { get; init; }

    /// <summary>
    /// Gets or sets the attach information.
    /// </summary>
    public required AttachInfo Attach { get; init; }

    /// <summary>
    /// Gets or sets the offset values.
    /// </summary>
    public required ImmutableArray<float> Offset { get; init; }

    /// <summary>
    /// Gets or sets the scale value.
    /// </summary>
    public required int Scale { get; init; }

    /// <summary>
    /// Gets or sets the entity face object information.
    /// </summary>
    public required AttachInfo EntityFaceObject { get; init; }

    /// <summary>
    /// Gets or sets the appear state.
    /// </summary>
    public required string AppearState { get; init; }

}






